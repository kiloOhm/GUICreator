namespace Oxide.Plugins
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Networking;

    public partial class GUICreator
    {
        public class ImageLoader : MonoBehaviour
        {

            public IEnumerator DownloadImage(string url, Action<byte[]> callback, int? sizeX = null, int? sizeY = null, Action ErrorCallback = null)
            {
                UnityWebRequest www = UnityWebRequest.Get(url);

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    PluginInstance.Puts(string.Format("Image failed to download! Error: {0}, Image URL: {1}", www.error, url));
                    www.Dispose();
                    ErrorCallback?.Invoke();
                    yield break;
                }

                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(www.downloadHandler.data);

                byte[] originalBytes = null;
                byte[] resizedBytes = null;

                if (texture != null)
                {
                    originalBytes = texture.EncodeToPNG();
                }
                else
                {
                    ErrorCallback?.Invoke();
                }

                if (sizeX != null && sizeY != null)
                {
                    resizedBytes = Resize(originalBytes, sizeX.Value, sizeY.Value, sizeX.Value, sizeY.Value, true);
                }

                if(originalBytes.Length <= resizedBytes.Length)
                {
                    callback(originalBytes);
                }
                else
                {
                    callback(resizedBytes);
                }

                www.Dispose();
            }

            //public static byte[] Resize(byte[] bytes, int sizeX, int sizeY)
            //{
            //    Image img = (Bitmap)(new ImageConverter().ConvertFrom(bytes));
            //    Bitmap cutPiece = new Bitmap(sizeX, sizeY);
            //    System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(cutPiece);
            //    graphic.DrawImage(img, new System.Drawing.Rectangle(0, 0, sizeX, sizeY), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
            //    graphic.Dispose();
            //    MemoryStream ms = new MemoryStream();
            //    cutPiece.Save(ms, ImageFormat.Jpeg);
            //    return ms.ToArray();
            //}

            public static byte[] Resize(byte[] bytes, int width, int height, int targetWidth, int targetHeight, bool enforceJpeg, RotateFlipType rotation = RotateFlipType.RotateNoneFlipNone)
            {
                byte[] resizedImageBytes;

                using (MemoryStream originalBytesStream = new MemoryStream(), resizedBytesStream = new MemoryStream())
                {
                    // Write the downloaded image bytes array to the memorystream and create a new Bitmap from it.
                    originalBytesStream.Write(bytes, 0, bytes.Length);
                    Bitmap image = new Bitmap(originalBytesStream);

                    if (rotation != RotateFlipType.RotateNoneFlipNone)
                    {
                        image.RotateFlip(rotation);
                    }

                    // Check if the width and height match, if they don't we will have to resize this image.
                    if (image.Width != targetWidth || image.Height != targetHeight)
                    {
                        // Create a new Bitmap with the target size.
                        Bitmap resizedImage = new Bitmap(width, height);

                        // Draw the original image onto the new image and resize it accordingly.
                        using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(resizedImage))
                        {
                            graphics.DrawImage(image, new System.Drawing.Rectangle(0, 0, targetWidth, targetHeight));
                        }


                        // Save the bitmap to a MemoryStream as either Jpeg or Png.
                        if (enforceJpeg)
                        {
                            resizedImage.Save(resizedBytesStream, ImageFormat.Jpeg);
                        }
                        else
                        {
                            resizedImage.Save(resizedBytesStream, ImageFormat.Png);
                        }

                        // Grab the bytes array from the new image's MemoryStream and dispose of the resized image Bitmap.
                        resizedImageBytes = resizedBytesStream.ToArray();
                        resizedImage.Dispose();
                    }
                    else
                    {
                        // The image has the correct size so we can just return the original bytes without doing any resizing.
                        resizedImageBytes = bytes;
                    }

                    // Dispose of the original image Bitmap.
                    image.Dispose();
                }

                // Return the bytes array.
                return resizedImageBytes;
            }
        }
    }
}