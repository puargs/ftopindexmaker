using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TopIndexMaker.Models;

namespace TopIndexMaker
{
    public class TopIndexMaker {

        /// <summary>
        /// The base directory for the top level index
        /// </summary>
        /// <returns></returns>
        public string baseDirectory {
            get {
                return _baseDirectory;
            }
            private set {
                _baseDirectory = value;
            }
        }

        private string _baseDirectory = "";

        /// <summary>
        /// The page title for the top level albums
        /// </summary>
        /// <returns></returns>
        public string baseTitle { get; set; }

        /// <summary>
        /// This is the default title to be used for any non-specified title.
        /// </summary>
        public const string DefaultTitle = "Albums";

        /// <summary>
        /// All albums
        /// </summary>
        private List<Album> albums;

        /// <summary>
        /// All html which will actually appear in index.html will be placed here.
        /// </summary>
        private StringBuilder html;

        /// <summary>
        /// The template for HTML manipulation from which to work
        /// </summary>
        private string templateHTML;

        /// <summary>
        /// The template for album-level div html manipulation from which to work
        /// </summary>
        private string templateBodySnippet;

        /// <summary>
        /// Storage location for the style.css self-reference stream. Should probably be moved elsewhere.
        /// </summary>
        private Stream styleInStream;


        /// <summary>
        /// All program-level option flags are stored here.
        /// </summary>
        public enum Options {
            preserveUnderscores,
            preserveIndexLink
        };

        private bool isPreservingUnderscores;
        private bool isPreservingIndexLinks;

        /// <summary>
        /// Default blank constructor.
        /// </summary>
        public TopIndexMaker() {
            loadEmbeddedData();
            resetInformation();
        }

        /// <summary>
        /// Default constructor for general use.
        /// </summary>
        /// <param name="baseDirectory">The base directory for index creation</param>
        /// <param name="baseTitle">(optional) The title of the base album</param>
        /// <param name="options">(optional) Array of any other specific options/flags</param>
        public TopIndexMaker(string baseDirectory, string baseTitle = DefaultTitle, params Options[] options) {
            this.baseDirectory = baseDirectory;
            this.baseTitle = baseTitle;

            loadEmbeddedData();
            resetInformation();

            foreach (Options option in options) {
                switch (option) {
                    case Options.preserveUnderscores:
                        this.isPreservingUnderscores = true;
                        break;
                    case Options.preserveIndexLink:
                        this.isPreservingIndexLinks = true;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Initialization/reset routine
        /// </summary>
        private void resetInformation() {
            albums = new List<Album>();
            isPreservingUnderscores = false;
            isPreservingIndexLinks = false;
            html = new StringBuilder();
        }

        /// <summary>
        /// Loads any embedded information/files from within the "Resources" folder which are necessary for execution.
        /// </summary>
        private void loadEmbeddedData() {
            var assembly = typeof(TopIndexMaker).GetTypeInfo().Assembly;
            string[] names = assembly.GetManifestResourceNames();

            using (Stream templateStream = assembly.GetManifestResourceStream("ftopindexmaker.Resources.htmlTemplate.html"))
            {
                using (StreamReader reader = new StreamReader(templateStream))
                {
                    templateHTML = reader.ReadToEnd();
                }
            }

            using (Stream templateStream = assembly.GetManifestResourceStream("ftopindexmaker.Resources.bodySnippet.html"))
            {
                using (StreamReader reader = new StreamReader(templateStream))
                {
                    templateBodySnippet = reader.ReadToEnd();
                }
            }

            styleInStream = assembly.GetManifestResourceStream("ftopindexmaker.Resources.style.css");
        }

        /// <summary>
        /// Default external entry point for starting the actual input/output mechanism. 
        /// Supports additional runs by specifying the baseDirectory.
        /// </summary>
        /// <param name="baseDirectory">(optional) The base directory for index creation</param>
        public void Start(string baseDirectory = null) {
            if (!string.IsNullOrEmpty(baseDirectory)) {
                this.baseDirectory = baseDirectory;
            }

            enumerateAlbums();
            setupHTML();
            outputAll();
        }

        /// <summary>
        /// Debug method, insert when necessary and use to check for problems.
        /// </summary>
        private void debugAlbums() {
            var count = 0;
            foreach (Album album in albums) {
                count++;
                Console.WriteLine("Album #" + count.ToString() + ": " + album.Title + " :: " + album.FirstImageFileUrl);
            }
        }

        /// <summary>
        /// Adds all albums to this.albums using the current baseDirectory.
        /// </summary>
        private void enumerateAlbums() {
            try {
                var dirs = Directory.EnumerateDirectories(baseDirectory);
                foreach (var dir in dirs) {
                    var album = getAlbum(dir);

                    if (album != null) {
                        albums.Add(album);
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Returns an album if directory is valid album. otherwise returns null.
        /// </summary>
        /// <param name="dir">The directory from which to fetch the album</param>
        /// <returns>The album, or null if invalid.</returns>
        private Album getAlbum(string dir) {
            var album = new Album(dir, isPreservingUnderscores);
            if (album.isValid) {
                return album;
            }
            return null;
        }

        /// <summary>
        /// Takes the actual albums and creates the index.html contents (stored in "html").
        /// </summary>
        private void setupHTML() {
            html.Append(templateHTML);
            html.Replace("{TITLE}", baseTitle);
            html.Replace("{BACKGROUND}", albums[0].BackgroundUrl);
            html.Replace("{NOISE}", albums[0].NoiseUrl);

            StringBuilder body = new StringBuilder();
            StringBuilder divBuilder = new StringBuilder();

            //yes, we do the sort here - allows for the background image to be "randomly" chosen through unsorted list indexing above.
            //probably a good candidate for a user choice via params flag
            albums = albums.OrderBy(d => d.Title).ToList();

            foreach (Album album in albums) {
                divBuilder.Clear();
                divBuilder.Append(templateBodySnippet);
                divBuilder.Replace("{LINK}", isPreservingIndexLinks ? album.Url + "index.html" : album.Url);
                divBuilder.Replace("{IMG}", album.FirstImageFileUrl);
                divBuilder.Replace("{TITLE}", album.Title);
                body.Append(divBuilder);
            }

            html.Replace("{BODY}", body.ToString());
        }

        /// <summary>
        /// Outputs the final index.html to file, along with style.css and wraps things up.
        /// </summary>
        private void outputAll() {
            try {
                string indexLocation = Path.Combine(baseDirectory, "index.html");
                File.WriteAllText(indexLocation, html.ToString());

                string styleLocation = Path.Combine(baseDirectory, "style.css");
                using (var styleOutStream = File.Create(styleLocation))
                {
                    styleInStream.Seek(0, SeekOrigin.Begin);
                    styleInStream.CopyTo(styleOutStream);
                }

                Console.WriteLine("Files written successfully! Check output folder: " + baseDirectory);
            }
            catch (Exception ex) {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

    }
}