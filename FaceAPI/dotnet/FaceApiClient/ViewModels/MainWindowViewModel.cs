using FaceApiClient.Extensions;
using FaceApiClient.Models;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FaceApiClient.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public ReactiveProperty<string> GroupName { get; }
        public ReactiveProperty<string> PersonName { get; }
        public ReactiveProperty<Person> SelectedPerson { get; }
        public ReactiveProperty<TrainStatus> TrainStatus { get; }
        public ReactiveCommand CreateGroup { get; }
        public ReactiveCommand AddPerson { get; }
        public ReactiveCommand RemovePerson { get; }
        public ReactiveCommand RefreshPersons { get; }
        public ReactiveCommand AddPersonFace { get; }
        public ReactiveCommand ClearPersonFace { get; }
        public ReactiveCommand TrainGroup { get; }
        public ReactiveCommand GetTrainStatus { get; }
        public ReactiveCommand DetectFaces { get; }
        public ReactiveCommand CloseResult { get; }
        public ReactiveProperty<bool> CanExecute { get; }
        public ReactiveProperty<bool> CanAddPersonExecute { get; }
        public ReactiveProperty<bool> CanAddPersonFaceExecute { get; }
        public ReactiveProperty<bool> ShowResult { get; }
        public ReactiveCollection<Person> Persons { get; }
        public ReactiveProperty<DetectResult> DetectResult { get; }
        private FaceIdentifyService FaceApi = new FaceIdentifyService(ConfigurationManager.AppSettings["CognitiveSubscriptionKey"]);
        private DrawingGroup FaceDrawingGroup = new DrawingGroup();
        public ReactiveProperty<BitmapImage> FaceResultImage;

        public MainWindowViewModel()
        {
            CanExecute = new ReactiveProperty<bool>(false);
            CanAddPersonExecute = new ReactiveProperty<bool>(false);
            CanAddPersonFaceExecute = new ReactiveProperty<bool>(false);
            ShowResult = new ReactiveProperty<bool>(false);
            TrainStatus = new ReactiveProperty<Models.TrainStatus>();
            DetectResult = new ReactiveProperty<DetectResult>();
            GroupName = new ReactiveProperty<string>(ConfigurationManager.AppSettings["GroupName"]);
            GroupName.Subscribe(x => {
                CanExecute.Value = !string.IsNullOrWhiteSpace(x);
            });
            PersonName = new ReactiveProperty<string>("");
            PersonName.Subscribe(x => {
                CanAddPersonExecute.Value = !string.IsNullOrWhiteSpace(x);
            });
            SelectedPerson = new ReactiveProperty<Person>();
            SelectedPerson.Subscribe(x =>
            {
                CanAddPersonFaceExecute.Value = x != null;
            });
            CloseResult = new ReactiveCommand();
            CloseResult.Subscribe(_ =>
            {
                ShowResult.Value = false;
            });

            //グループ追加
            CreateGroup = CanExecute.ToReactiveCommand(false);
            CreateGroup.Subscribe(async _ =>
            {
                await CreateGroupAsync();
            });

            //ユーザー一覧取得
            RefreshPersons = CanExecute.ToReactiveCommand(false);
            RefreshPersons.Subscribe(async _ =>
            {
                await RefreshPersonsAsync();
            });

            //ユーザー追加
            AddPerson = CanAddPersonExecute.ToReactiveCommand(false);
            AddPerson.Subscribe(async _ =>
            {
                await AddPersonAsync();
                await RefreshPersonsAsync();
            });

            //ユーザー削除
            RemovePerson = CanAddPersonExecute.ToReactiveCommand(false);
            RemovePerson.Subscribe(async _ =>
            {
                await RemovePersonAsync();
                await RefreshPersonsAsync();
            });

            //写真追加
            AddPersonFace = CanAddPersonFaceExecute.ToReactiveCommand(false);
            AddPersonFace.Subscribe(async f =>
            {
                await AddPersonFaceAsync(f as string);
                await RefreshPersonsAsync();
            });

            //写真削除
            ClearPersonFace = CanAddPersonFaceExecute.ToReactiveCommand(false);
            ClearPersonFace.Subscribe(async _ =>
            {
                await ClearPersonFaceAsync();
                await RefreshPersonsAsync();
            });

            //学習の実行
            TrainGroup = CanExecute.ToReactiveCommand(false);
            TrainGroup.Subscribe(async _ =>
            {
                await TrainGroupAsync();
            });

            //学習状態の取得
            GetTrainStatus = CanExecute.ToReactiveCommand(false);
            GetTrainStatus.Subscribe(async _ =>
            {
                await GetTrainStatusAsync();
            });
            //人物判定
            DetectFaces = CanExecute.ToReactiveCommand(false);
            DetectFaces.Subscribe(async f =>
            {
                await DetectFacesAsync(f as string);
            });

            FaceResultImage = new ReactiveProperty<BitmapImage>();

            Persons = new ReactiveCollection<Person>();
            RefreshPersonsAsync();

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                CanExecute.Value = true;
            }
        }

        private async Task CreateGroupAsync()
        {
            CanExecute.Value = false;
            try
            {
                var g = await FaceApi.ExistPersonGroupAsync(GroupName.Value);
                if (g)
                {
                    await DialogService.ShowMessage("既に存在しています");
                    return;
                }
                await FaceApi.CreatePersonGroupAsync(GroupName.Value, GroupName.Value);
                await DialogService.ShowMessage("作成しました");
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessage(ex.Message);
            }
            finally
            {
                CanExecute.Value = true;
            }
        }

        private async Task AddPersonAsync()
        {
            CanExecute.Value = false;
            try
            {
                if (await FaceApi.ExistPersonAsync(GroupName.Value, PersonName.Value))
                {
                    await DialogService.ShowMessage("既に存在しています");
                    return;
                }
                await FaceApi.AddPersonAsync(GroupName.Value, PersonName.Value);
                await DialogService.ShowMessage("追加しました");
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessage(ex.Message);
            }
            finally
            {
                CanExecute.Value = true;
            }
        }

        private async Task RemovePersonAsync()
        {
            CanExecute.Value = false;
            try
            {
                var ret = await FaceApi.GetPersonsAsync(GroupName.Value);
                foreach (var id in ret.Where(x => string.Compare(x.Name, PersonName.Value, false) == 0).Select(x => x.PersonId))
                {
                    await FaceApi.RemovePersonAsync(GroupName.Value, id);
                }
                await DialogService.ShowMessage("削除しました");
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessage(ex.Message);
            }
            finally
            {
                CanExecute.Value = true;
            }
        }

        private async Task RefreshPersonsAsync()
        {
            CanExecute.Value = false;
            try
            {
                var ret = await FaceApi.GetPersonsAsync(GroupName.Value);
                Persons.ClearOnScheduler();
                foreach (var p in ret)
                {
                    Persons.AddOnScheduler(p);
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessage(ex.Message);
            }
            finally
            {
                CanExecute.Value = true;
            }
        }

        private async Task AddPersonFaceAsync(string filepath)
        {
            CanExecute.Value = false;
            try
            {
                Guid ret;
                using (var s = File.OpenRead(filepath))
                {
                    ret = await FaceApi.AddPersonFaceAsync(GroupName.Value, SelectedPerson.Value.PersonId, s);
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessage(ex.Message);
            }
            finally
            {
                CanExecute.Value = true;
            }
        }

        private async Task ClearPersonFaceAsync()
        {
            CanExecute.Value = false;
            try
            {
                await FaceApi.ClearPersonFaceAsync(GroupName.Value, SelectedPerson.Value.PersonId);
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessage(ex.Message);
            }
            finally
            {
                CanExecute.Value = true;
            }
        }

        private async Task TrainGroupAsync()
        {
            CanExecute.Value = false;
            try
            {
                await FaceApi.TrainGroupAsync(GroupName.Value);
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessage(ex.Message);
            }
            finally
            {
                CanExecute.Value = true;
            }
        }

        private async Task GetTrainStatusAsync()
        {
            CanExecute.Value = false;
            try
            {
                var ret = await FaceApi.GetTrainStatusAsync(GroupName.Value);
                TrainStatus.Value = ret;
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessage(ex.Message);
            }
            finally
            {
                CanExecute.Value = true;
            }
        }

        private async Task DetectFacesAsync(string filepath)
        {
            CanExecute.Value = false;
            try
            {
                List<DetectFaces> ret;
                using (var s = File.OpenRead(filepath))
                {
                    ret = await FaceApi.DetectFacesAsync(GroupName.Value, s);
                }
                var m = ret.GroupJoin(Persons, l => l.PersonId, p => p.PersonId, (l, p) => new
                {
                    PersonId = l.PersonId,
                    Confidence = l.Confidence,
                    Position = l.Position,
                    Person = p.DefaultIfEmpty(new Person { Name = "不明", PersonId = Guid.Empty, PersistedFaceIds = null })
                })
                .SelectMany(a => a.Person.DefaultIfEmpty(), (a1, a2) => new DetectFaces
                {
                    PersonId = a1.PersonId,
                    Confidence = a1.Confidence,
                    Position = a1.Position,
                    Name = a2.Name
                })
                .ToList();

                UpdateResultImage(filepath, m);

                DetectResult.Value = new DetectResult
                {
                    SourceImagePath = filepath,
                    Faces = m
                };

                ShowResult.Value = true;
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessage(ex.Message);
            }
            finally
            {
                CanExecute.Value = true;
            }
        }

        private void UpdateResultImage(string filepath, List<DetectFaces> faces)
        {
            int width, height;
            using (var img = new System.Drawing.Bitmap(System.Drawing.Image.FromFile(filepath)))
            {
                width = img.Width;
                height = img.Height;
            }
            using (var dc = FaceDrawingGroup.Open())
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                    null,
                    new Rect(0.0, 0.0, width, height));
                foreach (var r in faces)
                {
                    dc.DrawRectangle(null,
                        new Pen(Brushes.Orange, 2),
                        new Rect(
                            r.Position.Left,
                            r.Position.Top,
                            r.Position.Width,
                            r.Position.Height)
                            );
                    dc.DrawText(new FormattedText(
                                    string.Format("{0}", r.Name),
                                    CultureInfo.GetCultureInfo("ja"),
                                    FlowDirection.LeftToRight,
                                    new Typeface("Meiryo"),
                                    30,
                                    Brushes.Orange),
                                new Point(r.Position.Left, r.Position.Top - 40)
                                );
                }
            }
        }
    }
}