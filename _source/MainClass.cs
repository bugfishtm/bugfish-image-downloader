using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace wib
{
    class ImageDownloader
    {
        ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////
        /// Reset Search Vars
        ////////////////////////////////////////////////////
            string contextcode = null;
            public void ResetVars() { this.contextcode = null;  }
            public bool getContext() { if (this.contextcode == null) { return false; } return true; }

        ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////
        /// Get a Complete HTML Website as String
        ////////////////////////////////////////////////////
            public void getHtmlUrlAsString(String URL)
            {
                if (URL == null) { return; }
                HttpWebRequest request;
                HttpWebResponse response;
                    try
                    {
                        request = (HttpWebRequest)WebRequest.Create(URL);
                        response = (HttpWebResponse)request.GetResponse();
                    }
                    catch (WebException) { return; }
                    catch (Exception) { return; }

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream;
                    if (String.IsNullOrWhiteSpace(response.CharacterSet)) { 
                        readStream = new StreamReader(receiveStream);}
                    else { 
                        try { 
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet)); 
                        } catch(ArgumentException) { 
                            readStream = new StreamReader(receiveStream, Encoding.UTF8); 
                        }
                    
                }
                    this.contextcode = readStream.ReadToEnd();
                    response.Close();
                    readStream.Close();
                    return;
                }

                response.Close();
                return;
            }

        ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////
        /// Download Images If Not Exists
        ////////////////////////////////////////////////////
        public string replacespecialchars(string StrinInit)
        {
            string newstring = StrinInit;
            newstring = newstring.Replace("?", "");
            return newstring;
        }

            ////////////////////////////////////////////////////
            ////////////////////////////////////////////////////
            /// Download Images If Not Exists
            ////////////////////////////////////////////////////
            public bool downloadAnImage(string filename, string imageUrl, bool HiresBool)
            {
                string[] tmpformatstring = imageUrl.Split('/');
                ImageFormat format;


            string tmpendingforfile = tmpformatstring[tmpformatstring.Length - 1];
            bool checkboolforendingvar = false;
            bool foundendinginstring = tmpendingforfile.IndexOf("jpg") != -1;
            if (foundendinginstring) { checkboolforendingvar = true;tmpendingforfile = "jpg"; }

            foundendinginstring = tmpendingforfile.LastIndexOf("pn") != -1;
            if (foundendinginstring) { checkboolforendingvar = true;tmpendingforfile = "png"; }

            foundendinginstring = tmpendingforfile.LastIndexOf("png") != -1;
            if (foundendinginstring) { checkboolforendingvar = true;tmpendingforfile = "png"; }

            foundendinginstring = tmpendingforfile.LastIndexOf("PNG") != -1;
            if (foundendinginstring) { checkboolforendingvar = true;tmpendingforfile = "png"; }

            foundendinginstring = tmpendingforfile.LastIndexOf("jpeg") != -1;
            if (foundendinginstring) { checkboolforendingvar = true;tmpendingforfile = "jpeg"; }

            foundendinginstring = tmpendingforfile.LastIndexOf("bmp") != -1;
            if (foundendinginstring) { checkboolforendingvar = true;tmpendingforfile = "bmp"; }

            foundendinginstring = tmpendingforfile.LastIndexOf("gif") != -1;
            if (foundendinginstring) { checkboolforendingvar = true;tmpendingforfile = "gif"; }

            foundendinginstring = tmpendingforfile.LastIndexOf("ico") != -1;
            if (foundendinginstring) { checkboolforendingvar = true;tmpendingforfile = "ico"; }

            foundendinginstring = tmpendingforfile.LastIndexOf("JPG") != -1;
            if (foundendinginstring) { checkboolforendingvar = true;tmpendingforfile = "jpg"; }

            foundendinginstring = tmpendingforfile.LastIndexOf("JPEG") != -1;
            if (foundendinginstring) { checkboolforendingvar = true;tmpendingforfile = "jpeg"; }

            foundendinginstring = tmpendingforfile.LastIndexOf("BMP") != -1;
            if (foundendinginstring) { checkboolforendingvar = true;tmpendingforfile = "bmp"; }

            foundendinginstring = tmpendingforfile.LastIndexOf("GIF") != -1;
            if (foundendinginstring) { checkboolforendingvar = true;tmpendingforfile = "gif"; }

            foundendinginstring = tmpendingforfile.LastIndexOf("ICO") != -1;
            if (foundendinginstring) { checkboolforendingvar = true;tmpendingforfile = "ico"; }


            if (imageUrl.Substring(0, 4) != "http" || !checkboolforendingvar) { return false; }

                if (File.Exists(filename + "." + tmpendingforfile)) { return true; }
                WebClient client = new WebClient();

            Stream stream;
            try
            {
                stream = client.OpenRead(imageUrl);
            }
            catch (WebException) { return false; }
            catch (Exception) { return false; }

            Bitmap bitmap;
            try
            {
                bitmap = new Bitmap(stream);
            } catch(ArgumentException)
            {
                stream.Flush();
                stream.Close();
                client.Dispose();
                return false;
            }


            if (bitmap != null) { 
                    if(HiresBool)
                        {
                            if(bitmap.Height > 400 || bitmap.Width > 400)
                            {
                                bitmap.Save(filename + "." + tmpendingforfile, bitmap.RawFormat);
                            } else {
                                stream.Flush();
                                stream.Close();
                                client.Dispose();
                                return false;
                            }
                        } else  {
                            bitmap.Save(filename + "." + tmpendingforfile, bitmap.RawFormat);
                        }
                   
                    stream.Flush();
                    stream.Close();
                    client.Dispose();
                    return true; }
                else {
                    stream.Flush();
                    stream.Close();
                    client.Dispose();
                    return false; }
            }

        ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////
        /// GET IMAGE LINKS FROM CONTEXT
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public List<string> getImageLinks(String URLDomain)
        {
            List<string> tmpsitearray = new List<string>();

            string linksearchvar = "src=\"";
            string linkendvar = "\"";

            string currentstring = this.contextcode;
            currentstring = currentstring.Replace("\n", " ");
            currentstring = currentstring.Replace("  ", " ");

            while (currentstring.IndexOf(linksearchvar) != -1)
            {
                currentstring = currentstring.Substring(currentstring.IndexOf(linksearchvar) + linksearchvar.Length);

                string tmplinkstring = currentstring;
                tmplinkstring = tmplinkstring.Substring(0, tmplinkstring.IndexOf(linkendvar));
                currentstring = currentstring.Substring(currentstring.IndexOf(linkendvar));

                if (tmplinkstring != null && tmplinkstring.Length > 4)
                {
                    if (tmplinkstring.Substring(0, 4) == "http")
                    {
                        tmpsitearray.Add(tmplinkstring);
                    }
                    else
                    {
                        if (tmplinkstring.Length > 4)
                        {
                            tmpsitearray.Add(URLDomain + tmplinkstring);
                        }
                    }
                }
            }


            return tmpsitearray;
        }

        ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////
        /// GET SITE LINKS FROM CONTEXT
        ////////////////////////////////////////////////////
        public List<string> getSiteLinks(String LastSearchString, String URLDomain)
        {
            List<string> tmpsitearray = new List<string>();
            tmpsitearray.Add(LastSearchString);
            
            string linksearchvar = "<a href=\"";
            string linkendvar    = "\"";

            string currentstring = this.contextcode;
            currentstring = currentstring.Replace("\n", " ");
            currentstring = currentstring.Replace("  ", " ");

            while (currentstring.IndexOf(linksearchvar) != -1) {
               currentstring = currentstring.Substring(currentstring.IndexOf(linksearchvar) + linksearchvar.Length);

               string tmplinkstring = currentstring;
               tmplinkstring = tmplinkstring.Substring(0, tmplinkstring.IndexOf(linkendvar));
               currentstring = currentstring.Substring(currentstring.IndexOf(linkendvar));

                if (tmplinkstring != null && tmplinkstring.Length > 5)
                {
                    if (tmplinkstring.Substring(0, 4) == "http")
                    {
                        tmpsitearray.Add(tmplinkstring);
                    }
                    else
                    {
                        if (tmplinkstring.Length > 4)
                        {
                            tmpsitearray.Add(URLDomain + tmplinkstring);
                        }
                    }
                }
            }
            return tmpsitearray;
        }

    }
}