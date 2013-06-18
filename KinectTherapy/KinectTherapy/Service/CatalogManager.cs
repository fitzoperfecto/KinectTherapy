using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Kinect.Toolbox.Record;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework.Storage;
using System.Xml;

namespace SWENG.Service
{
    public enum CatalogManagerStatus
    {
        CatalogReady,
        FileNotFound,
        SelectStart,
        SelectStop
    }

    public delegate void CatalogSelectionStatusChanged(object sender, CatalogSelectionChangedEventArg e);

    public class CatalogSelectionChangedEventArg : EventArgs
    {
        public string ExerciseId;

        public CatalogSelectionChangedEventArg(string exerciseId)
        {
            ExerciseId = exerciseId;
        }
    }

    public class CatalogManager : IGameComponent
    {

        #region event stuff
        public event CatalogSelectionStatusChanged CatalogSelectionStatusChanged;

        // Invoke the Changed event; called whenever the catalog status changes
        protected virtual void OnRecordingStatusChanged(CatalogSelectionChangedEventArg e)
        {
            if (CatalogSelectionStatusChanged != null)
                CatalogSelectionStatusChanged(this, e);
        }

        #endregion

        private string _catalogFile;
        public string CatalogFile { get; set; }

        public CatalogManagerStatus Status { get; internal set; }

        public CatalogManager()
        {

            // Initialize catalog variables 
            var applicationDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _catalogFile = applicationDirectory + @"\Catalog\ExerciseCatalog.xml";

            CatalogFile = _catalogFile;

            if (!File.Exists(_catalogFile))
            {
                //TODO: Handle File Not Found exception
                var catalogManagerStatus = CatalogManagerStatus.FileNotFound;
            }
           
        }

        public void Initialize()
        {
            Status = CatalogManagerStatus.CatalogReady;
        }

        public void SelectionStart()
        {
            Status = CatalogManagerStatus.SelectStart;
        }

        public void SelectionStop(object sender, EventArgs e)
        {
            Status = CatalogManagerStatus.SelectStop;
        }
    }
}
