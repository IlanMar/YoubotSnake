using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace VRepClient
{
    public class OdomFromCam
    {
       public  string txb_text="";
      public   float[] CamOdomData = new float[3];

        public float[] getCamOdom()
        {
            string site = "http://10.0.48.214:7000/coords";

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(site);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            using (StreamReader stream = new StreamReader(
                 resp.GetResponseStream(), Encoding.UTF8))
            {
                txb_text = stream.ReadToEnd();
            }

            List<XAttribute> xmlList = new List<XAttribute>();
            //CamOdomData = new float[3];
            string s = txb_text;
            XmlDocument xm = new XmlDocument();
            xm.LoadXml(string.Format("<root>{0}</root>", s)); 
            String xmlString = txb_text;
            XDocument xdoc = XDocument.Load(new StringReader(xmlString));
            xmlList = (from pt in xdoc.Descendants("pt")
                           select pt.Attribute("xya")).ToList();
            string xmstring = Convert.ToString(xmlList[1]);
            string str1;
            string str2 = "";
            Decimal number;
            
            
            string[] words = xmstring.Split(new char[] { '"' });//парсим строку в массив words
            string[] words2 = words[1].Split(new char[] { ';' });//парсим строку в массив words
            for (int i = 0; i < 3; i++)
            {
                CamOdomData[i] = float.Parse(words2[i], System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
            }

            txb_text = s;


            return CamOdomData;
        }

    }
}
