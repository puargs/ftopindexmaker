using System.IO;
using System.Linq;

namespace TopIndexMaker.Models
{
    public class Album {

        /// <summary>
        /// Contains the title of the album.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Contains the path to the physical folder where the album is located on disk.
        /// Set: when this variable is changed, the Title will automatically update to reflect it.
        /// </summary>
        /// <returns></returns>
        public string Folder { 
            get { return _Folder; }
            set {
                _Folder = value;
                setDefaultTitle(isPreservingUnderscores);
                InitializeAndVerify();
            } 
        }
        private string _Folder = "";

        /// <summary>
        /// Contains the relative url to the background-blurred image.
        /// </summary>
        /// <returns></returns>
        public string BackgroundUrl { get; private set; }

        /// <summary>
        /// Contains the relative url to the first image in the album set.
        /// </summary>
        /// <returns></returns>
        public string FirstImageFileUrl { get; private set; }

        /// <summary>
        /// Returns true if the album is valid. Do not use the album if this is not true.
        /// </summary>
        /// <returns></returns>
        public bool isValid {
            get {
                return _isValid;
            }
            private set {
                _isValid = value;
            }
        }
        private bool _isValid = false;

        /// <summary>
        /// Reflects the current state of the program's options for preserving underscores
        /// </summary>
        private bool isPreservingUnderscores = false;

        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public Album() {
            
        }

        /// <summary>
        /// This constructor will initialize an Album and update the Title based on the folder name.
        /// </summary>
        /// <param name="Folder">The physical path to the album's holding folder</param>
        /// <param name="preserveUnderscores">Set to true to preserve underscores for the album.</param>
        public Album(string Folder, bool preserveUnderscores = false) {
            this.isPreservingUnderscores = preserveUnderscores;
            this.Folder = Folder;
        }

        /// <summary>
        /// Sets the default title for the album. Called when the folder is set.
        /// </summary>
        /// <param name="preserveUnderscores">Set to true to preserve underscores for the album.</param>
        private void setDefaultTitle(bool preserveUnderscores) {
            this.Title = Path.GetFileName(this.Folder);

            if (!preserveUnderscores) {
                this.Title = this.Title.Replace("_", " ");
            }
        }
        
        /// <summary>
        /// Initializes internal variables for the album and checks to make sure it's valid on disk. 
        /// </summary>
        public void InitializeAndVerify() {
            isValid = false;
            string folderName = Path.GetFileName(Folder);
            string thumbsFolder = Path.Combine(Folder, "thumbs");
            string indexFile = Path.Combine(Folder, "index.html");
            if (File.Exists(indexFile) && Directory.Exists(thumbsFolder)) {
                var files = Directory.EnumerateFiles(thumbsFolder).ToList();
                if (files.Count > 0) {
                    BackgroundUrl = folderName + "/blurs/" + Path.GetFileName(files[0]);
                    FirstImageFileUrl = folderName + "/thumbs/" + Path.GetFileName(files[0]);
                    isValid = true;
                }
            }
        }

        /// <summary>
        /// Returns the relative URL for the album
        /// </summary>
        /// <returns></returns>
        public string Url {
            get {
                return Path.GetFileName(Folder) + "/";
            }
        }

        /// <summary>
        /// Returns the relative URL for the background-noise image for this album.
        /// </summary>
        /// <returns></returns>
        public string NoiseUrl {
            get {
                return Path.GetFileName(Folder) + "/noise.png";
            }
        }
    }
}