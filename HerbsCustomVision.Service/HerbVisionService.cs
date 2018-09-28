using Microsoft.Cognitive.CustomVision.Prediction;
using Microsoft.Cognitive.CustomVision.Prediction.Models;
using Microsoft.Cognitive.CustomVision.Training;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HerbsCustomVision.Service
{
    public class HerbVisionService
    {

        public HerbVisionService(string trainingKey, string predictionKey)
        {
            _trainingApi = new TrainingApi() { ApiKey = trainingKey };
            _predictionEndpoint = new PredictionEndpoint() { ApiKey = predictionKey };

            var projects = _trainingApi.GetProjects();
            _herbProject = projects.FirstOrDefault(p => p.Name == "Herbs");
        }

        private PredictionEndpoint _predictionEndpoint;
        private TrainingApi _trainingApi;
        private Project _herbProject;

        public IList<ImageTagPredictionModel> Predict(Stream imageFile)
        {

            if (_herbProject != null)
            {
                var result = _predictionEndpoint.PredictImage(_herbProject.Id, imageFile);

                return result.Predictions;
            }
            else
            {
                Console.WriteLine("Project doesn't exist.");
                return null;
            }
        }

        private void Train()
        {
            Console.WriteLine("Input path to image to train model with:");
            var imagePath = Console.ReadLine();

            Console.WriteLine("What tag would you give this image? Rosemary, cilantro, or basil?");
            var imageTag = Console.ReadLine();

            var capitilizedTag = char.ToUpper(imageTag.First()) + imageTag.Substring(1).ToLower();

            if (!File.Exists(imagePath))
            {
                Console.WriteLine("File does not exist. Press enter to exit.");
                Console.ReadLine();
                return;
            }

            var imageFile = File.OpenRead(imagePath);

            var tags = _trainingApi.GetTags(_herbProject.Id);

            var matchedTag = tags.Tags.FirstOrDefault(t => t.Name == capitilizedTag);

            var memoryStream = new MemoryStream();
            imageFile.CopyTo(memoryStream);

            var fileCreateEntry = new ImageFileCreateEntry(imageFile.Name, memoryStream.ToArray());
            var fileCreateBatch = new ImageFileCreateBatch { Images = new List<ImageFileCreateEntry> { fileCreateEntry }, TagIds = new List<Guid> { matchedTag.Id } };

            var result = _trainingApi.CreateImagesFromFiles(_herbProject.Id, fileCreateBatch);

            var resultImage = result.Images.FirstOrDefault();

            switch (resultImage.Status)
            {
                case "OKDuplicate":
                    Console.WriteLine("Image is already used for training. Please use another to train with");
                    Console.ReadLine();
                    break;
                default:
                    break;
            }

            var iteration = _trainingApi.TrainProject(_herbProject.Id);

            while (iteration.Status != "Completed")
            {
                System.Threading.Thread.Sleep(1000);

                iteration = _trainingApi.GetIteration(_herbProject.Id, iteration.Id);
            }

            iteration.IsDefault = true;
            _trainingApi.UpdateIteration(_herbProject.Id, iteration.Id, iteration);
            Console.WriteLine("Done training!");

            Console.ReadLine();
        }
    }
}
