using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDSDKLib;
using System.Threading;
using System.Diagnostics;
using System.Drawing;

namespace CanonDll
{
    public class Control
    {
        IntPtr Camera,archivo;
        public String CaptureFile;
        private CallbackTakePhoto TakePhotoCallback;
        private EDSDK.EdsObjectEventHandler mObjectEventCallBack;
        string Ruta, Nombre;
        int lcdX, lcdY, screen;
        uint lcdSize;
        string data, data2;
        Topaz.SigPlusNET sigPlusNET1 = new Topaz.SigPlusNET();         

        public class EDSFileObject
        {
            public String mPath;
            public IntPtr mFilePointer;
            public EDSDK.EdsDirectoryItemInfo mFileInfo;


            public EDSFileObject(String Path, IntPtr filepointer, EDSDK.EdsDirectoryItemInfo fileinfo)
            {
                mPath = Path;
                mFilePointer = filepointer;
                mFileInfo = fileinfo;
            }
        }
        ~Control()
        {
            Disconnect();
        }
        public void Connect(string RutaArchivo,string NombreArchivo)
        {
            Console.WriteLine("entrando de connect");
            IntPtr CameraList;
            Ruta = RutaArchivo;
            Nombre = NombreArchivo;

            EDSDK.EdsInitializeSDK();
            Console.WriteLine("inicializadoooo");
            EDSDK.EdsGetCameraList(out CameraList);
            EDSDK.EdsGetChildAtIndex(CameraList, 0, out Camera);
            EDSDK.EdsOpenSession(Camera);
            IntPtr outPropertyData = new IntPtr();           
            //EDSDK.EdsGetPropertyData(Camera,EDSDK.PropID_FlashOn,0,0,outPropertyData);
            EDSDK.EdsSetPropertyData(Camera, EDSDK.PropID_FlashOn, 0, 1, outPropertyData);
            mObjectEventCallBack = new EDSDK.EdsObjectEventHandler(ObjectEventCallBack);
            EDSDK.EdsSetObjectEventHandler(Camera, EDSDK.ObjectEvent_All, mObjectEventCallBack, IntPtr.Zero);
            Console.WriteLine("saliendo de connect");
        }
        public void Disconnect()
        {
           // mObjectEventCallBack = new EDSDK.EdsObjectEventHandler(ObjectEventCallBack);
           // EDSDK.EdsSetObjectEventHandler(Camera, EDSDK.ObjectEvent_All, mObjectEventCallBack, IntPtr.Zero);
            EDSDK.EdsCloseSession(Camera);
            EDSDK.EdsRelease(Camera);
            EDSDK.EdsTerminateSDK();
        }

        public uint StateEventCallBack(uint Event, UInt32 Parameter, IntPtr Context)
        {
            return EDSDK.EDS_ERR_OK;
        }

        public uint PropertyEventCallBack(uint Event, uint Property, uint x, IntPtr Context)
        {
            return EDSDK.EDS_ERR_OK;
        }

        public uint ObjectEventCallBack(uint Event, IntPtr Object, IntPtr Context)
        {
            Console.WriteLine("Filepath--" + Event.ToString());
            switch (Event)
            {
                case EDSDK.ObjectEvent_DirItemCreated:
                    List<EDSFileObject> Results = ListAll();
                    foreach (EDSFileObject File in Results)
                    {
                        if (File.mFileInfo.isFolder == 0)
                        {
                            //String Filepath = "c:\\m\\" + File.mPath;
                            String Filepath = Ruta+Nombre+".jpg";
                            String FolderPath = System.IO.Path.GetDirectoryName(Filepath);
                            if (!System.IO.Directory.Exists(FolderPath))
                                System.IO.Directory.CreateDirectory(FolderPath);
                            Console.WriteLine("Archivo: " + Filepath.ToString());
                            if (!System.IO.File.Exists(Filepath))
                            {
                                DownloadImage(Filepath, File.mFilePointer);
                                if (TakePhotoCallback != null)
                                    TakePhotoCallback(Filepath);
                            }
                        }
                    }
                    break;             

            }
            return EDSDK.EDS_ERR_OK;
        }

        public List<EDSFileObject> ExpandAll(String Path, IntPtr Folder)
        {
            List<EDSFileObject> results = new List<EDSFileObject>();
            int Count;
            EDSDK.EdsGetChildCount(Folder, out Count);
            EDSDK.EdsDirectoryItemInfo FileItemInfo;
            IntPtr FileItem;
            for (int i = 0; i < Count; i++)
            {
                EDSDK.EdsGetChildAtIndex(Folder, i, out FileItem);
                EDSDK.EdsGetDirectoryItemInfo(FileItem, out FileItemInfo);
                results.Add(new EDSFileObject(Path + "\\" + FileItemInfo.szFileName, FileItem, FileItemInfo));

                if (FileItemInfo.isFolder > 0)
                {
                    List<EDSFileObject> temps = ExpandAll(Path + "\\" + FileItemInfo.szFileName, FileItem);
                    results.AddRange(temps);
                }
            }
            return results;
        }

        public List<EDSFileObject> ListAll()
        {
            IntPtr Volumn;
            EDSDK.EdsGetChildAtIndex(Camera, 0, out Volumn);
            return ExpandAll("", Volumn);
        }

        public delegate uint CallbackTakePhoto(String Path);

        public void TakePicture()
        {
            Console.WriteLine("--Foto--");

            //Control.CallbackTakePhoto tf = takeFoto;

            //EDSDK.EdsSendCommand(Camera, EDSDK.CameraCommand_TakePicture, 0);
           // TakePhotoCallback = tf;
            //Console.WriteLine("saliendo take");
            Thread.Sleep(200);
            List<EDSFileObject> Results = ListAll();
            foreach (EDSFileObject File in Results)
            {                
                if (File.mFileInfo.isFolder == 0)
                {
                    //String Filepath = "c:\\m\\" + File.mPath;
                    String Filepath = Ruta + Nombre + ".jpg";
                    String FolderPath = System.IO.Path.GetDirectoryName(Filepath);
                    if (!System.IO.Directory.Exists(FolderPath))
                        System.IO.Directory.CreateDirectory(FolderPath);
                    /*Console.WriteLine("Archivo: " + File.mFileInfo.szFileName.ToString());
                    Console.WriteLine("Archivo 1: " + File.mFilePointer.ToString());
                    Console.WriteLine("Archivo 2: " + File.mPath.ToString());*/
                    //if (!System.IO.File.Exists(Filepath))
                    //{
                        DownloadImage(Filepath, File.mFilePointer);
                        if (TakePhotoCallback != null)
                            TakePhotoCallback(Filepath);
                   // }
                }               
            }

        }

        public uint takeFoto(string file)
        {
            Console.WriteLine("takefoto"+file.ToString());
            //MessageBox.Show(file);
            //if (file.Substring(file.Length - 3, 3).ToUpper() == "JPG")
           // {
               // pictureBox1.Image = Image.FromFile(file);
            //}
            return 0;
        }

        public void DeletePictures()
        {
            Console.WriteLine("Entrando DeletePictures");

            Control.CallbackTakePhoto tf = takeFoto;                     
            TakePhotoCallback = tf;
            List<EDSFileObject> Results = ListAll();
            foreach (EDSFileObject File in Results)
            {
                EDSDK.EdsDeleteDirectoryItem(File.mFilePointer);                
            }
            Console.WriteLine("saliendo DeletePictures");
        }

        public void DownloadImage(String Path, IntPtr DirItem)
        {
            Console.WriteLine("DownloadImage");

            EDSDK.EdsDirectoryItemInfo DirInfo;

            EDSDK.EdsGetDirectoryItemInfo(DirItem, out DirInfo);

            IntPtr Stream;
            EDSDK.EdsCreateFileStream(Path, EDSDK.EdsFileCreateDisposition.CreateAlways, EDSDK.EdsAccess.ReadWrite, out Stream);

            EDSDK.EdsDownload(DirItem, DirInfo.Size, Stream);

            EDSDK.EdsDownloadComplete(DirItem);

            EDSDK.EdsRelease(Stream);

        }
        public void ActivarFirmadora()
        {
            try
            {
                sigPlusNET1.SetTabletState(1); //Turns tablet on to collect signature
                sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
                sigPlusNET1.SetTranslateBitmapEnable(false);

                //Get LCD size in pixels.
                lcdSize = sigPlusNET1.LCDGetLCDSize();
                lcdX = (int)(lcdSize & 0xFFFF);
                lcdY = (int)((lcdSize >> 16) & 0xFFFF);

                sigPlusNET1.ClearSigWindow(1);
                sigPlusNET1.SetLCDCaptureMode(2);
                sigPlusNET1.ClearSigWindow(1);

                sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
                Font f = new System.Drawing.Font("Arial", 9.0F, System.Drawing.FontStyle.Regular);
                sigPlusNET1.LCDWriteString(0, 2, 35, 25, f, "Bienvenido a Maquilas Tetakawi.");
                System.Threading.Thread.Sleep(1000);
                sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
            }
            catch(Exception e)
            {
            }
        }
        
        public void DescargarImgFirma(string nombre)
        {
            //abrir
            sigPlusNET1.ClearSigWindow(1);
            sigPlusNET1.SetSigCompressionMode(0);
            sigPlusNET1.AutoKeyStart();
            sigPlusNET1.AutoKeyFinish();
            sigPlusNET1.SetEncryptionMode(0);
            sigPlusNET1.SetImageXSize(235);
            sigPlusNET1.SetImageYSize(124);
            sigPlusNET1.SetJustifyMode(0);
            sigPlusNET1.SetImagePenWidth(3);
            sigPlusNET1.SetImageFileFormat(0);
            Image myimage = sigPlusNET1.GetSigImage();
            myimage.Save(nombre);
            
            sigPlusNET1.LCDRefresh(1, 210, 3, 14, 14); //Refresh LCD at 'OK' to indicate to user that this option has been sucessfully chosen

            if (sigPlusNET1.NumberOfTabletPoints() > 0)
            {
                sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
                Font f = new System.Drawing.Font("Arial", 9.0F, System.Drawing.FontStyle.Regular);
                sigPlusNET1.LCDWriteString(0, 2, 35, 25, f, "Captura Completada.");
                sigPlusNET1.ClearTablet();
                System.Threading.Thread.Sleep(1000);
                sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);

                //Refresh for close
                sigPlusNET1.KeyPadClearHotSpotList();

                sigPlusNET1.SetLCDCaptureMode(1);
                sigPlusNET1.SetTabletState(0);
                
            }
        }      

    }
}
