using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Emotion;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string pathToImage = string.Empty;
            OpenFileDialog openImageFileDialog = new OpenFileDialog();

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openImageFileDialog.InitialDirectory = desktopPath;
            openImageFileDialog.Filter = "Pictures (*.jpg)|*.jpg|All files (*.*)|*.*";
            openImageFileDialog.RestoreDirectory = true;

            label1.Text = "";
            if (openImageFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pathToImage = openImageFileDialog.FileName;
                    Bitmap aBitmap = new Bitmap(pathToImage);
                    pictureBox1.Image = aBitmap;

                    FaceDetection(pathToImage);
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        public async void FaceDetection(string pathToFile)
        {
            string subscriptionKeyFace = "insert Face API Key here";
            string subscriptionKeyEmo = "insert Emotions API Key here";

            FaceServiceClient fSC = new FaceServiceClient(subscriptionKeyFace);

            EmotionServiceClient eSC = new EmotionServiceClient(subscriptionKeyEmo);

            using (Stream s = File.OpenRead(pathToFile))
            {
            var requiredFaceAttributes = new FaceAttributeType[] {
                FaceAttributeType.Age,
                FaceAttributeType.Gender,
                FaceAttributeType.Smile,
                FaceAttributeType.FacialHair,
                FaceAttributeType.HeadPose,
                FaceAttributeType.Glasses
                };

                var faces = await fSC.DetectAsync(s,
                                                  returnFaceLandmarks: true,
                                                  returnFaceAttributes: requiredFaceAttributes);

                label1.Text = "Number of Faces:" + faces.Length;

                var faceRectangles = new List<Microsoft.ProjectOxford.Common.Rectangle>();

                foreach (var face in faces)
                {
                    var rect = face.FaceRectangle;

                    var landmarks = face.FaceLandmarks;

                    var age = face.FaceAttributes.Age;
                    var gender = face.FaceAttributes.Gender;
                    var glasses = face.FaceAttributes.Glasses;


                    var rectangle = new Microsoft.ProjectOxford.Common.Rectangle
                    {
                        Height = face.FaceRectangle.Height,
                        Width = face.FaceRectangle.Width,
                        Top = face.FaceRectangle.Top,
                        Left = face.FaceRectangle.Left,
                    };
                    faceRectangles.Add(rectangle);


                    try
                    {
                        label2.Text = "Age: " + age;
                        Console.Write("Age: " + age);

                        label3.Text = "Gender: " + gender;
                        label5.Text = "Does the person wear glasses? " + glasses;
                    }
                    catch (Exception e)
                    {
                        Console.Write(e);
                    }
                    
                    if (faces.Length > 0)
                    {
                        using (Stream str = File.OpenRead(pathToFile))
                        {
                            var emotions = await eSC.RecognizeAsync(str, faceRectangles.ToArray());
                            
                            string emotionsList = "";

                            foreach (var emotion in emotions)
                            {
                                emotionsList += $@"Anger: {emotion.Scores.Anger}
                                    Contempt: {emotion.Scores.Contempt}
                                    Disgust: {emotion.Scores.Disgust}
                                    Fear: {emotion.Scores.Fear}
                                    Happiness: {emotion.Scores.Happiness}
                                    Neutral: {emotion.Scores.Neutral}
                                    Sadness: {emotion.Scores.Sadness}                                    
                                    Surprise: {emotion.Scores.Surprise}";
                            }

                            label4.Text = emotionsList;
                        }
                    }
                }
            }
        }
    }
}
