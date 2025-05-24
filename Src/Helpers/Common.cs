using Avalonia.Media.Imaging;
using Tsundoku.ViewModels;

namespace Tsundoku.Helpers
{
    public class Common
    {
        /// <summary>
        /// Based on proveded image url, where customImageUrl takes precidence generate a Avalonia BitMap from that link 
        /// </summary>
        /// <param name="newPath">The path to the image file in the "Covers" directory on the users machine</param>
        /// <param name="coverLink">Original image url from a API call</param>
        /// <param name="customImageUrl">Web image url provided by the user</param>
        /// <returns></returns>
        public static async Task<Bitmap?> GenerateAvaloniaBitmap(string newPath, string coverLink = "", string customImageUrl = "")
        {
            Bitmap newCover;
            byte[] imageByteArray;
            try
            {
                if (!string.IsNullOrWhiteSpace(customImageUrl) && (customImageUrl.EndsWith("jpg") || customImageUrl.EndsWith("png")) && Uri.TryCreate(customImageUrl, UriKind.RelativeOrAbsolute, out Uri uri))
                {
                    imageByteArray = await MainWindowViewModel.AddCoverHttpClient.GetByteArrayAsync(uri);
                }
                else
                {
                    imageByteArray = await MainWindowViewModel.AddCoverHttpClient.GetByteArrayAsync(coverLink);
                }
                Stream imageStream = new MemoryStream(imageByteArray);
                newCover = new Bitmap(imageStream).CreateScaledBitmap(new PixelSize(LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);
                newCover.Save(newPath, 100);
                imageStream.Flush();
                imageStream.Close();
                return newCover;
            }
            catch (Exception ex)
            {
                LOGGER.Error("{} => \n {}", ex.Message, ex.StackTrace);
            }
            return null;
        }
    }
}