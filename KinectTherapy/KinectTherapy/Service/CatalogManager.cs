using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
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

    public class CatalogItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public CatalogItem() { }

        public CatalogItem(string id, string name, string description)
        {
            ID = id;
            Name = name;
            Description = description;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }
            CatalogItem ci = obj as CatalogItem;
            return (this.ID == ci.ID);// && this.Name == ci.Name && this.Description == ci.Description);
        }

        public override string ToString()
        {
            return "ID: " + ID + " NAME: " + Name + " DESC: " + Description;
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

        public string CatalogFile { get; set; }

        /// <summary>
        /// The current list of workouts selected. Generated into exercise objects once completed.
        /// This is what the UI would be populating/depopulating when assigning exercises to the workout.
        /// Maybe this should be bound to in the UI?
        /// </summary>
        private List<Exercise> _workoutList;

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
                                // do start case
                                break;
                            }
                        case CatalogManagerStatus.Complete:
                            {
                                OnCatalogComplete(GetCurrentWorkout());
                                break;
                            }
                    }
                }
                _status = value;
            }
        }

        private string CatalogDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "../../../../KinectTherapyContent/Exercises/";
        private const string XmlHeader = @"<?xml version=""1.0"" encoding=""utf-8"" ?>";

        private string _exerciseGroup { get; set; }

        /// <summary>   
        /// The master list of exercises
        /// </summary>
        public DataTable DataTable { get; internal set; }

        public CatalogManager()
        {
            _status = CatalogManagerStatus.Start;
            // Initialize catalog variables 
            var applicationDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            CatalogFile = applicationDirectory + "../../../../KinectTherapyContent/Exercises/MasterCatalog.xml";
            _workoutList = new List<Exercise>();
            // load the datatable.
            CatalogXmlLinqData();
        }

        public void Initialize()
        {
        }

        /// <summary>
        /// Pulls the xml from the MasterCatalog and adds to a DataTable object.
        /// 
        /// The DataTable will contain the master list of exercises.
        /// </summary>
        public void CatalogXmlLinqData()
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
            DataTable.Columns.Add("Category");
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
        }

        /// <summary>
        /// Add an exercise to the current list if it does not exist already
        /// </summary>
        /// <param name="exerciseId"></param>
        public void AddExerciseToSelected(string exerciseId)
        {
            foreach (Exercise exercise in _workoutList)
            {
                if (exercise.Id == exerciseId)
                {
                    return;
                }
            }

            _workoutList.Add(
                new Exercise()
                {
                    Id = exerciseId
                }
            );
        }

        public void SetExerciseOptions(Exercise exerciseUpdate)
        {
            for (int i = 0; i < _workoutList.Count; i = i +1)
            {
                if (_workoutList[i].Id == exerciseUpdate.Id)
                {
                    _workoutList[i] = exerciseUpdate;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exerciseId"></param>
        /// <returns></returns>
        public Exercise GetExercise(string exerciseId)
        {
            Exercise r = null;

            foreach (Exercise exercise in _workoutList)
            {
                if (exercise.Id == exerciseId)
                {
                    r = exercise;
                }
            }

            return r;
        }

        /// <summary>
        /// Remove an exercise from the current list if it does exist
        /// </summary>
        /// <param name="exerciseId"></param>
        public void RemoveExerciseFromSelected(string exerciseId)
        {
            int i = 0;
            foreach (Exercise exercise in _workoutList)
            {
                if (exercise.Id == exerciseId)
                {
                    break;
                }
                i = i + 1;
            }

            _workoutList.RemoveAt(i);
        }

        /// <summary>
        /// This will generate the Exercise objects which will be overlayed on the default Exercises loaded in the ExerciseQueue
        /// Used by the OnCatalogComplete event
        /// </summary>
        /// <returns></returns>
        public CatalogCompleteEventArg GetCurrentWorkout()
        {
            Exercise[] exerciseList = _workoutList.ToArray();
            CatalogCompleteEventArg eventArgs = new CatalogCompleteEventArg(exerciseList);
            return eventArgs;
        }

        /// <summary>
        /// Retrieves the Exercises by Exercise Group it should probably return more than just the ids, 
        /// but it will work for now.
        /// </summary>
        /// <param name="exerciseGroup"></param>
        /// <returns></returns>
        public List<CatalogItem> GetExercisesByType(string exerciseGroup)
        {
            List<CatalogItem> r = new List<CatalogItem>();
            try
            {
                var dataRow = DataTable.Select("Category = '" + exerciseGroup + "'");
                foreach (var row in dataRow)
                {
                    r.Add(
                        new CatalogItem()
                        {
                            ID = row["Id"].ToString(), 
                            Name = row["Name"].ToString(), 
                            Description = row["Description"].ToString()
                        }
                    );
                }
            }
            catch (Exception)
            {
                r = null;
                throw;
            }

            return r;
        }
    }
}
