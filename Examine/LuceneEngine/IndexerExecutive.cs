﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace Examine.LuceneEngine
{

    /// <summary>
    /// Manages the delegation of authority over which machine in a load balanced environment will perform the indexing.
    /// This is done by an IO race on initialization of the LuceneExamineIndexer.
    /// If a server's app pool is recycled at a seperate time than the rest of the servers in the cluster, it will generally
    /// take over the executive role (this is dependant on the time that the last latest server's app pool was restarted).
    /// The Executive is determined by file lock (.lck) file, theoretically there should only be one of these.
    /// If there is only 1 server in the cluster, then obviously it is the Executive.
    /// </summary>
    public sealed class IndexerExecutive : IDisposable
    {

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="d"></param>
        public IndexerExecutive(DirectoryInfo d)
        {
            ExamineDirectory = d;
            _exaFile = new FileInfo(Path.Combine(ExamineDirectory.FullName, Environment.MachineName + ExaExtension));
            _lckFile = new FileInfo(Path.Combine(ExamineDirectory.FullName, Environment.MachineName + LckExtension));

            //new 10 minute timer
            _timestampTimer = new Timer(new TimeSpan(0, 10, 0).TotalMilliseconds);
            _timestampTimer.AutoReset = true;
            _timestampTimer.Elapsed += new ElapsedEventHandler(TimestampTimer_Elapsed);
        }

        public DirectoryInfo ExamineDirectory { get; private set; }

        private readonly FileInfo _exaFile;
        private readonly FileInfo _lckFile;
        private readonly Timer _timestampTimer;

        private const string TimeStampFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private const string LckExtension = ".lck";
        private const string ExaExtension = ".exa";

        private static readonly object Lock = new object();

        public enum EXAFields
        {
            Name, Created, Updated, IsMaster
        }
        public enum LCKFields
        {
            Name, Created, Updated
        }

        /// <summary>
        /// Determines if the executive has been initialized. 
        /// This is useful for checking if files have been deleted during website operations.
        /// </summary>
        /// <returns></returns>
        public bool IsInitialized()
        {
            if (_exaFile == null || _lckFile == null)
            {
                return false;
            }
            _exaFile.Refresh();
            _lckFile.Refresh();
            if (!_exaFile.Exists || !_lckFile.Exists)
            {
                return false;
            }

            return true;
        }

        ///<summary>
        /// Runs initialization
        ///</summary>
        public void Initialize()
        {
            CreateEXAFile();
            
            //only clear lock files older than 1 hour as there may already be an active Executive
            ClearOldLCKFiles(DateTime.Now.AddHours(-1));
            //only clear exa files older than 1 hour as this file is updated every 10 mins
            ClearOldEXAFiles(DateTime.Now.AddHours(-1));

            RaceForMasterIndexer();

            //start the timestamp timer
            _timestampTimer.Start();
        }

        /// <summary>
        /// Fired every 10 minutes by the timer object. This timestamps the EXA file to 
        /// enure the system knows that this server is active.
        /// This is to ensure that all systems in a Load Balanced environment are aware of exactly how
        /// many other servers are taking part in the load balancing and who they are.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TimestampTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckDisposed();

            TimestampEXA();
        }

        /// <summary>
        /// Ensures there is an elected Executive, otherwise starts the race. 
        /// Returns a bool as to whether or not this is the Executive machine.
        /// </summary>
        public bool IsExecutiveMachine
        {
            get
            {
                int count = LCKFileCount();
                if (count != 1)
                {
                    //since we know there's no Executive (or somehow more than 1 have been declared), 
                    //clear all lock files and start the race.
                    ClearOldLCKFiles(DateTime.Now);
                    RaceForMasterIndexer();
                }
                else
                {
                    //update machine's files with new timestamp
                    TimestampLck();
                    TimestampEXA();
                }

                //if the lck file exists with this machine name, then it is executive.
                _lckFile.Refresh();
                return _lckFile.Exists;
            }
        }

        /// <summary>
        /// Returns a boolean determining whether or not this server involved in a LoadBalanced
        /// environment with Umbraco Examine.
        /// </summary>
        public bool IsLoadBalancedEnvironment
        {
            get
            {
                return EXAFileCount() > 1;
            }
        }

        /// <summary>
        /// Returns the machine name of the executive indexer
        /// </summary>
        public string ExecutiveIndexerMachineName
        {
            get
            {
                _lckFile.Refresh();
                if (_lckFile.Exists)
                {
                    return GetLCK()[LCKFields.Name];
                }
                return "";
            }
        }

        /// <summary>
        /// The number of servers active in indexing
        /// </summary>
        public int ServerCount
        {
            get
            {
                return EXAFileCount();
            }
        }

        /// <summary>
        /// Creates an xml file to declare that this machine is taking part in the index writing.
        /// This is used to determine the master indexer if this app exists in a load balanced environment.
        /// </summary>
        private void CreateEXAFile()
        {
            lock (Lock)
            {
                var d = new Dictionary<EXAFields, string>();
                d.Add(EXAFields.Name, Environment.MachineName);
                d.Add(EXAFields.Created, DateTime.Now.ToString(TimeStampFormat));
                d.Add(EXAFields.Updated, DateTime.Now.ToString(TimeStampFormat));
                d.Add(EXAFields.IsMaster, false.ToString());
                d.SaveToDisk(_exaFile); 
            }
        }

        /// <summary>
        /// Creates a lock file for this machine if there aren't other ones.
        /// </summary>
        /// <returns>returns true if a lock file was successfully created for this machine.</returns>
        private bool CreateLCKFile()
        {
            lock (Lock)
            {
                if (GetOtherLCKFiles().Count == 0)
                {
                    var lckFileName = Environment.MachineName + LckExtension;
                    var fLck = new FileInfo(Path.Combine(ExamineDirectory.FullName, lckFileName));
                    var dLck = new Dictionary<LCKFields, string>();
                    dLck.Add(LCKFields.Name, Environment.MachineName);
                    dLck.Add(LCKFields.Created, DateTime.Now.ToString(TimeStampFormat));
                    dLck.Add(LCKFields.Updated, DateTime.Now.ToString(TimeStampFormat));

                    //check one more time
                    if (GetOtherLCKFiles().Count == 0)
                    {
                        dLck.SaveToDisk(fLck);
                        return true;
                    }
                } 
            }

            return false;
        }

        /// <summary>
        /// delete all old lck files (any that are more than cutoffTime old)
        /// </summary>
        /// <param name="cutoffTime"></param>
        private void ClearOldLCKFiles(DateTime cutoffTime)
        {
            lock (Lock)
            {
                ExamineDirectory
                        .GetFiles(LckExtension)
                        .Where(x => x.CreationTime < cutoffTime)
                        .ToList()
                        .ForEach(x => x.Delete()); 
            }
        }

        /// <summary>
        /// delete all old exa files (any that are more than cutoffTime old)
        /// </summary>
        /// <param name="cutoffTime"></param>
        private void ClearOldEXAFiles(DateTime cutoffTime)
        {
            lock (Lock)
            {
                ExamineDirectory
                        .GetFiles(ExaExtension)
                        .Where(x => x.CreationTime < cutoffTime)
                        .ToList()
                        .ForEach(x => x.Delete()); 
            }
        }

        /// <summary>
        /// Get all lck files that are not named by this machines name. If there are any, this means that another machine
        /// has won the race and created the lck file for itself. If there is a lck file with the current machines name, then this
        /// must mean it was previously the master indexer and the apppool has recycled in less than the hour.
        /// </summary>
        /// <returns></returns>
        private List<FileInfo> GetOtherLCKFiles()
        {
            return ExamineDirectory
                .GetFiles(LckExtension)
                .Where(x => !x.Name.StartsWith(Environment.MachineName))
                .ToList();
        }

        private int LCKFileCount()
        {
            return ExamineDirectory
               .GetFiles("*" + LckExtension)
               .Count();
        }

        private int EXAFileCount()
        {
            return ExamineDirectory
               .GetFiles("*" + ExaExtension)
               .Count();
        }

        /// <summary>
        /// Updates the timestamp for lck file if it exists
        /// </summary>
        private void TimestampLck()
        {
            lock (Lock)
            {
                _lckFile.Refresh();
                if (_lckFile.Exists)
                {
                    var lck = GetLCK();
                    lck[LCKFields.Updated] = DateTime.Now.ToString(TimeStampFormat);
                    lck.SaveToDisk(_lckFile);
                }            
            } 
        }

        /// <summary>
        /// Updates the timestamp for the exa file
        /// </summary>
        private void TimestampEXA()
        {
            lock (Lock)
            {
                var exa = GetEXA();
                exa[EXAFields.Updated] = DateTime.Now.ToString(TimeStampFormat);
                exa.SaveToDisk(_exaFile); 
            }
        }

        /// <summary>
        /// Read the machines EXA file
        /// </summary>
        /// <returns></returns>
        private Dictionary<EXAFields, string> GetEXA()
        {
            var dExa = new Dictionary<string, string>();
            dExa.ReadFromDisk(_exaFile);
            return dExa.ToDictionary(x => (EXAFields)Enum.Parse(typeof(EXAFields), x.Key), x => x.Value);
        }

        /// <summary>
        /// Read the machines LCK file
        /// </summary>
        /// <returns></returns>
        private Dictionary<LCKFields, string> GetLCK()
        {
            var dLck = new Dictionary<string, string>();
            dLck.ReadFromDisk(_lckFile);
            return dLck.ToDictionary(x => (LCKFields)Enum.Parse(typeof(LCKFields), x.Key), x => x.Value);
        }

        /// <summary>
        /// This will check for any lock files, not created by the current machine. If there are any, then this machine will flag it's
        /// exa file as not being the master indexer, otherwise, it will try to create it's own lock file to let others know it is the race
        /// winner and therefore the master indexer. If this succeeds, it will update it's exa file to flag it as the master indexer.
        /// </summary>
        private void RaceForMasterIndexer()
        {
            lock (Lock)
            {
                //get this machine's exa file
                var dExa = GetEXA();

                if (CreateLCKFile())
                {
                    dExa[EXAFields.IsMaster] = true.ToString();
                }
                else
                {
                    dExa[EXAFields.IsMaster] = false.ToString();
                }
                dExa[EXAFields.Updated] = DateTime.Now.ToString(TimeStampFormat);
                dExa.SaveToDisk(_exaFile); 
            }

        }
        #region IDisposable Members

        private bool _disposed;

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("UmbracoExamine.IndexerExecutive");
            }
        }

        /// <summary>
        /// When the object is disposed, all data should be written
        /// </summary>
        public void Dispose()
        {
            this.CheckDisposed();
            this.Dispose(true);
            GC.SuppressFinalize(this);
            this._disposed = true;
        }

        private void Dispose(bool disposing)
        {
            this.CheckDisposed();
            if (disposing)
                this._timestampTimer.Dispose();
        }

        #endregion
    }
}
