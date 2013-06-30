using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using System.IO;
using System.Xml;
using SWENG.Criteria;

namespace SWENG.Service
{
    public enum CatalogManagerStatus
    {
        Complete,
        Start,
        Cancel
    }

    /// <summary>
    /// Internal Event which handles selection events performed by the UI layer
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void CatalogSelectionStatusChanged(object sender, CatalogSelectionChangedEventArg e);

    public class CatalogSelectionChangedEventArg : EventArgs
    {
        public string ExerciseId;

        public CatalogSelectionChangedEventArg(string exerciseId)
        {
            ExerciseId = exerciseId;
        }
    }

    /// <summary>
    /// Used by any Class which wants to know the Catalog creation has completed. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void CatalogCompleteEventHandler(object sender, CatalogCompleteEventArg e);

    public class CatalogCompleteEventArg : EventArgs
    {
        public Exercise[] Exercises;

        public CatalogCompleteEventArg(Exercise[] exercises)
        {
            Exercises = exercises;
        }
    }

    public class CatalogManager : IGameComponent
    {

        #region event stuff
        
        public event CatalogCompleteEventHandler CatalogCompleteEventHandler;
        // Invoke the Completion event; calle whenever the catalog creation has completed.
        protected virtual void OnCatalogComplete(CatalogCompleteEventArg e)
        {
            if (CatalogCompleteEventHandler != null)
            {
                CatalogCompleteEventHandler(this, e);
            }
        }

        public event CatalogSelectionStatusChanged CatalogSelectionStatusChanged;
        // Invoke the Changed event; called whenever the catalog status changes
        protected virtual void OnRecordingStatusChanged(CatalogSelectionChangedEventArg e)
        {
            if (CatalogSelectionStatusChanged != null)
            {
                CatalogSelectionStatusChanged(this, e);
            }
        }

        #endregion

        private string _catalogFile;
        public string CatalogFile { get; set; }

        private List<string> _workoutList;

        private CatalogManagerStatus _status;
        public CatalogManagerStatus Status
        {
            get { return _status; }

            internal set
            {
                if (_status != value)
                {
                    switch (value)
                    {
                        case CatalogManagerStatus.Cancel:
                            {
                                // do cancel case
                                break;
                            }
                        case CatalogManagerStatus.Start:
                            {
                                break;
                            }
                        case CatalogManagerStatus.Complete:
                            {
                                OnCatalogComplete(GetCurrentWorkout());
                                break;
                            }
                    }
                }
            }
        }

        private string CatalogDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "../../../../KinectTherapyContent/Exercises/";
        private const string XmlHeader = @"<?xml version=""1.0"" encoding=""utf-8"" ?>";

        private string _exerciseGroup { get; set; }

        /// <summary>
        /// The current list of workouts selected. Generated into exercise objects once completed.
        /// </summary>
        public List<string> WorkoutList { get; set; }

        /// <summary>
        /// Should be used by the UI to retreive the list of exercises per category.
        /// 
        /// Grabbed by DataTable.Select("Category = '" + exerciseGroup + "'");
        /// </summary>
        public DataTable DataTable { get; set; }

        public CatalogManager()
        {

            // Initialize catalog variables 
            var applicationDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _catalogFile = applicationDirectory +  "../../../../KinectTherapyContent/Exercises/MasterCatalog.xml";

            CatalogFile = _catalogFile;
            CatalogXmlLinqData();

        }

        public void Initialize()
        {
        }

        public void SelectionStart()
        {
            Status = CatalogManagerStatus.Start;
        }

        public void SelectionStop(object sender, EventArgs e)
        {
            Status = CatalogManagerStatus.Complete;
        }

        public DataTable CatalogXmlLinqData()
        {
            var catalogList =
                from c in XDocument.Load(CatalogDirectory + @"MasterCatalog.xml").Descendants("Exercise")
                select new
                {
                    xmlFile = c.Attribute("Id").Value
                };

            var xmlFileList = catalogList.ToList();
            var xmlFileName = new List<string>(xmlFileList.Count);
            xmlFileName.AddRange(from t in xmlFileList where !t.xmlFile.Equals("MasterCatalog") select t.xmlFile);

            DataTable = new DataTable("ExerciseCatalog");

            DataTable.Columns.Add("Id");
            DataTable.Columns.Add("Catagory");
            DataTable.Columns.Add("Name");
            DataTable.Columns.Add("Description");

            foreach (var fileName in xmlFileName)
            {
                var newFileName = CatalogDirectory + @"\" + fileName + ".xml";

                var query =
                    from s in XDocument.Load(CatalogDirectory + @"\MasterCatalog.xml").Descendants("Exercise")
                    join e in XDocument.Load(newFileName).Descendants("Exercise") on s.Attribute("Id").Value equals
                        e.Attribute("Id").Value

                    select new
                    {
                        Id = s.Attribute("Id").Value,
                        Category = e.Attribute("Category").Value,
                        Name = s.Attribute("Name").Value,
                        Description = e.Attribute("Description").Value
                    };

                foreach (var s in query)
                {
                    DataTable.Rows.Add(s.Id, s.Category, s.Name, s.Description);
                }
            }

            return DataTable;
        }

        public Exercise[] ListWorkoutExercises()
        {
            Exercise[] workout = new Exercise[WorkoutList.Count()];
            int i=0;
            foreach (var exercise in WorkoutList)
            {
                Exercise e = new Exercise();
                e.Id = exercise;
                workout[i++] = e;
            }

            return workout;
        }

        public XmlDocument ListWorkoutExerciseObjects(IEnumerable<string> exerciseList)
        {
            var fileContents = String.Empty;
            var startPos = 0;

            foreach (var sr in exerciseList.Select(item => new FileStream(CatalogDirectory + item + @".xml", FileMode.Open, FileAccess.Read)).Select(fs => new StreamReader(fs)))
            {
                fileContents = fileContents.Insert(startPos, sr.ReadToEnd());

                startPos = fileContents.Length;
            }

            // Remove all occurences of Xml Header that was inserted due to processing multiple files
            fileContents = fileContents.Replace(XmlHeader, "");

            // Put header back in for proper xml formatting
            fileContents = fileContents.Insert(0, XmlHeader);

            // Add root node
            fileContents = fileContents.Insert(XmlHeader.Length, "<Exercises>");
            fileContents = fileContents + "</Exercises>";

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(fileContents);

            // Notify the system that exercise selection is complete
            Status = CatalogManagerStatus.Complete;

            return xmlDocument;
        }

        /// <summary>
        /// This will generate the Exercise objects which will be overlayed on the default Exercises loaded in the ExerciseQueue
        /// Used by the OnCatalogComplete event
        /// </summary>
        /// <returns></returns>
        public CatalogCompleteEventArg GetCurrentWorkout()
        {
            var exerciseList = ListWorkoutExercises();
            var eventArgs = new CatalogCompleteEventArg(exerciseList);
            return eventArgs;
        }
    }
}
