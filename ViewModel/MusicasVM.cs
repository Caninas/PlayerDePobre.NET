using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlayerDePobre.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TagLib;

namespace PlayerDePobre.ViewModel
{
    public partial class MusicasVM : BaseVM
    {
        public MusicasVM()
        {
            Title = "Player de Pobre";
        }

        MediaElement player;

        string pasta_playlists = "";
        string pasta_selecionada = "";
        string pasta_tocando = "";

        int indexMusicaAtual = 0;

        bool aleatorio = true;

        readonly string pathCache = FileSystem.Current.CacheDirectory;
        readonly Random random = new();

        List<string> extensoes = new()
                    { ".MP3", ".M4A", ".AAC", ".FLAC", ".OGG", ".OGA", ".DOLBY", ".WAV", ".CAF", ".OPUS", ".WEBA"};


        public ObservableCollection<Musica> Musicas { get; set; } = new();
        public ObservableCollection<Musica> MusicasTocando { get; set; } = new();
        public ObservableCollection<Playlist> Playlists { get; set; } = new();

        [ObservableProperty]
        string musicaTocando = "";

        [ObservableProperty]
        ImageSource coverAtual;

        [ObservableProperty]
        Musica musicaAtual;

        [RelayCommand]
        private void InitMediaElement(MediaElement elemento)
        {
            player = elemento;
            player.MediaEnded += (sender, args) => { Proximo(); };
            player.MediaFailed += (sender, args) => { Proximo(); App.Current.MainPage.DisplayAlert("Erro", "Erro ao tocar música atual", "OK"); };
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
        Task SelecionarPlaylist(Playlist playlist)
        {
            if (playlist == null || playlist.Path == pasta_selecionada)
            {
                return Task.CompletedTask; ;
            }

            LerMusicasPlaylist(playlist.Path);
            return Task.CompletedTask;
        }

        [RelayCommand]
        private void TocarMusicaOnTap(Musica musica = null)
        {
            Debug.Write("a", nameof(musica));
            if (pasta_selecionada != pasta_tocando)
            {
                pasta_tocando = pasta_selecionada;
                MusicasTocando = new(Musicas.ToArray());

            }

            TocarMusicaPath(musica);
        }


        private void AtualizarCamposMusica(Musica musica=null)
        {
            if (musica != null)
            {
                MusicaAtual = musica;
                //CoverAtual = musica.Cover;
            }
            else
            {
                var albumPic = TagLib.File.Create(MusicaTocando).Tag.Pictures;


                if (albumPic.Length > 0)
                {
                    var memory = new MemoryStream(albumPic[0].Data.Data);
                    CoverAtual = ImageSource.FromStream(() => memory);
                }
                else
                {
                    CoverAtual = ImageSource.FromFile("padrao.png");
                }
            }

        }

        private void TocarMusicaPath(Musica musica)
        {                
            MusicaTocando = musica.FullPath;
            AtualizarCamposMusica(musica);
            player.MediaOpened += (sender, args) => { player.Play(); };

        }

        [RelayCommand]
        private async void TocarMusica(string path=null)
        {
            if (MusicaTocando != "")
            {
                if (player.CurrentState != MediaElementState.Playing) // is playing
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
                    MusicasTocando = new(Musicas.ToArray());
                    pasta_tocando = pasta_selecionada;

                    if (aleatorio)
                    {
                        indexMusicaAtual = random.Next(MusicasTocando.Count);
                        path = Path.Combine(pasta_selecionada, MusicasTocando[indexMusicaAtual].NomeOnDisk);
                    }
                    else
                    {
                        indexMusicaAtual = 0;
                        path = Path.Combine(pasta_selecionada, MusicasTocando[0].NomeOnDisk);
                    }

                    MusicaTocando = path;
                    AtualizarCamposMusica(MusicasTocando[indexMusicaAtual]);
                    player.MediaOpened += (sender, args) => { player.Play(); };
                }
            }
        }

        private async void Configuraçoes(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ConfigPage");
        }

        [RelayCommand]
        private void Anterior()
        {

        }

        [RelayCommand]
        private void Proximo()
        {
            if (MusicaTocando != "")
            {
                //string path;
                if (aleatorio)
                {
                    indexMusicaAtual = random.Next(MusicasTocando.Count);
                    //path = Path.Combine(pasta_tocando, MusicasTocando[indexMusicaAtual].NomeOnDisk);

                }
                else
                {
                    //path = Path.Combine(pasta_tocando, MusicasTocando[indexMusicaAtual++].NomeOnDisk);
                }

                TocarMusicaPath(MusicasTocando[indexMusicaAtual]);
            }
            return;
        }

        [RelayCommand]
        Task LerMusicasPlaylist(string pasta)
        {
            if (IsBusy)
            {
                return Task.CompletedTask;
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
                        }

                        thumb = ImageSource.FromFile(fullpath);
                        img = ImageSource.FromFile(fullpath);


                    }
                    else
                    {
                        img = ImageSource.FromFile("padrao.png");
                        thumb = img;
                    }

                    Musicas.Add(new Musica() { Album = tags.Album, Titulo = tags.Title, Artista = tags.FirstPerformer, Cover = img, CoverThumb=thumb, Extensao = extensao, FullPath = musica, NomeOnDisk = Path.GetFileName(musica)});


                }
            }

            Title = Path.GetFileName(pasta_selecionada);
            IsBusy = false;
            return Task.CompletedTask;
        }
    }
}
