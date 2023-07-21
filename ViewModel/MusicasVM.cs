using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlayerDePobre.Model;
using Plugin.Maui.Audio;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using TagLib.Matroska;


namespace PlayerDePobre.ViewModel
{
    public partial class MusicasVM : BaseVM
    {
        private readonly IAudioManager playersManager;
        private IAudioPlayer player;

        string pasta_playlists = "";
        string pasta_selecionada = "";
        string pasta_tocando = "";

        string musica_tocando = "";
        string musica_selecionada = "";

        bool aleatorio = true;

        readonly string pathCache = FileSystem.Current.CacheDirectory;
        readonly Random random = new();

        List<string> extensoes = new()
                    { ".MP3", ".M4A", ".AAC", ".FLAC", ".OGG", ".OGA", ".DOLBY", ".WAV", ".CAF", ".OPUS", ".WEBA"};


        public ObservableCollection<Musica> Musicas { get; set; } = new();
        public ObservableCollection<Playlist> Playlists { get; set; } = new();

        [ObservableProperty]
        ImageSource coverAtual;

        public MusicasVM(IAudioManager audioManager)
        {
            Title = "Player de Pobre";
            this.playersManager = audioManager;
        }

        [RelayCommand]
        async Task SelecionarPasta()
        {
            CancellationToken Ctoken = new();
            var pasta_picker = await FolderPicker.Default.PickAsync(Ctoken);
            ;
            if (pasta_picker.IsSuccessful)
            {
                pasta_playlists = pasta_picker.Folder.Path;

                Playlists.Clear();
                foreach (string pasta in Directory.GetDirectories(pasta_playlists))
                {
                    Playlists.Add(new Playlist() { Nome = Path.GetFileName(pasta), Path = pasta });
                }
                return;
            }
        }

        [RelayCommand]
        async Task SelecionarPlaylist(Playlist playlist)
        {
            if (playlist == null || playlist.Path == pasta_selecionada)
            {
                return;
            }

            LerMusicasPlaylist(playlist.Path);
            Debug.Write("a");
            return;
        }

        [RelayCommand]
        private void TocarMusica(Musica musica=null)
        {
            if (player != null)
            {
                if (!player.IsPlaying)
                {
                    player.Play();
                }
                else
                {
                    player.Pause();
                }
            }
            else
            {
                if (pasta_selecionada != "")
                {
                    string path;
                    if (musica == null) 
                    {
                        if (aleatorio)
                        {
                            path = Path.Combine(pasta_selecionada, Musicas[random.Next(Musicas.Count)].NomeOnDisk);
                        }
                        else
                        {
                            path = Path.Combine(pasta_selecionada, Musicas[0].NomeOnDisk);
                        }
                    }
                    else
                    {
                        path = Path.Combine(pasta_selecionada, musica.NomeOnDisk);
                    }

                    var memory = new MemoryStream(TagLib.File.Create(path).Tag.Pictures[0].Data.Data);
                    CoverAtual = ImageSource.FromStream(() => memory);
                    
                    player = playersManager.CreatePlayer(System.IO.File.OpenRead(path));
                    player.Play();
                }
            }
            //return Task.CompletedTask;
        }

        private void AlterarVolume(object sender, EventArgs e)
        {
            if (player != null)
            {
                Button btn = sender as Button;

                if (btn.Text == "+")
                {
                    if (this.player.Volume < 1)
                    {
                        this.player.Volume += 0.1;
                    }
                }
                else if (btn.Text == "-")
                {
                    if (this.player.Volume > 0)
                    {
                        this.player.Volume -= 0.1;
                    }
                }
            }
        }

        private async void Configuraçoes(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ConfigPage");
        }

        private void Anterior(object sender, EventArgs e)
        {

        }

        private void Proximo(object sender, EventArgs e)
        {

        }

        [RelayCommand]
        async Task LerMusicasPlaylist(string pasta)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            pasta_selecionada = pasta;
            Musicas.Clear();
            string extensao;

            //for (int i = 0; i <600; i++)
            foreach (string musica in Directory.GetFiles(pasta_selecionada))
            {
                extensao = Path.GetExtension(musica).ToUpper();
                if (extensoes.Contains(extensao))
                {
                    var tags = TagLib.File.Create(musica).Tag;
                    ImageSource img, thumb;//new StreamImageSource();
                    

                    if (tags.Pictures.Length > 0)
                    {
                        var fullpath = Path.Combine(pathCache, String.Format("{0}.jpg", Path.GetFileNameWithoutExtension(musica)));
                        if (!Path.Exists(fullpath))
                        {
                            var memory = new MemoryStream(tags.Pictures[0].Data.Data);

                            SixLabors.ImageSharp.Image thumbTemp = SixLabors.ImageSharp.Image.Load(memory);

                            thumbTemp.Mutate(x => x.Resize(100, 100));
                            thumbTemp.Save(fullpath);
                            thumbTemp.Dispose();

                            //FileStream outputStream = File.OpenWrite(fullpath);

                            //outputStream.Write(memory.ToArray(), 0, memory.ToArray().Length);
                            //outputStream.Close();

                        }

                        thumb = ImageSource.FromFile(fullpath);
                        img = ImageSource.FromFile(fullpath);


                    }
                    else
                    {
                        img = ImageSource.FromFile("padrao.png");
                        thumb = img;
                    }

                    Musicas.Add(new Musica() { Album = tags.Album, Titulo = tags.Title, Artista = tags.FirstPerformer, Cover = img, CoverThumb=thumb, Extensao = extensao, NomeOnDisk = Path.GetFileName(musica)});


                }
            }

            Title = Path.GetFileName(pasta_selecionada);
            IsBusy = false;
            return;
        }
    }
}
