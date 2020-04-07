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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Security;

namespace Boneworks_Customizer {

    public partial class Form1 : Form {
        
        public Form1 () {

            InitializeComponent ();

            SetDefaultNullbody ();
            SetDefaultNightvision ();
            SetDefaultRedDot ();
            SetDefaultFord ();

        }
        
        private void pathButton_Click (object sender, EventArgs e) {
            
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog ();
            if (folderBrowserDialog.ShowDialog () == DialogResult.OK) {
                pathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
            
        }

        private bool ColorButtonClick (ref Button button) {

            ColorDialog colorDialog = new ColorDialog ();
            colorDialog.AllowFullOpen = true;
            colorDialog.AnyColor = true;
            colorDialog.SolidColorOnly = true;
            colorDialog.Color = button.BackColor;

            if (colorDialog.ShowDialog () == DialogResult.OK) {

                button.BackColor = colorDialog.Color;
                return true;

            }

            return false;

        }
        
        private bool GetOutputDir (string subfolder, out string dir) {
            
            // Get the directory
            dir = pathTextBox.Text + @"\" + (subfolder);
            if (!Extensions.IsValidPath (dir)) {

                MessageBox.Show ("Export directory is not a valid path!", "Export error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;

            }

            // Create the directory if it doesn't exist
            if (!Directory.Exists (dir))
                Directory.CreateDirectory (dir);

            return true;

        }

        private void OpenHelp () {
            
            System.Diagnostics.Process.Start (@"https://www.olezen.xyz/boneworkscustomizer/help");
            
        }

        #region Nullbodies

        private void bullbodyBodyColorButton_Click (object sender, EventArgs e) {
            
            if (ColorButtonClick (ref nullbodyBodyColorButton))
                RefreshNullbodyImage ();
            
        }

        private void woundColorButton_Click (object sender, EventArgs e) {
            
            if (ColorButtonClick (ref nullbodyWoundColorButton))
                RefreshNullbodyImage ();
            
        }

        private void nullbodyWireframeColorButton_Click (object sender, EventArgs e) {
            
            if (ColorButtonClick (ref nullbodyWireframeColorButton))
                RefreshNullbodyImage ();
            
        }
        
        private void nullbodyExportButton_Click (object sender, EventArgs e) {
            
            ExportNullbodyMats (false);
            
        }

        private void corruptedNullbodyExportButton_Click (object sender, EventArgs e) {

            ExportNullbodyMats (true);

        }

        private void nullbodyTextureType_SelectedIndexChanged (object sender, EventArgs e) {
            
            RefreshNullbodyImage ();
            
        }

        private void nullbodyHelpButton_Click (object sender, EventArgs e) {

            OpenHelp ();

        }

        private void defaultNullbodyButton_Click (object sender, EventArgs e) {

            SetDefaultNullbody ();

        }

        private void defaultCorruptNullbodyButton_Click (object sender, EventArgs e) {

            SetDefaultCorruptNullbody ();

        }

        private void SetDefaultNullbody  () {

            nullbodyTextureType.SelectedIndex = 0;
            nullbodyBodyColorButton.BackColor = Color.FromArgb (255, 140, 74);
            nullbodyWoundColorButton.BackColor = Color.FromArgb (221, 197, 189);
            nullbodyWireframeColorButton.BackColor = Color.FromArgb (230, 230, 230);
            RefreshNullbodyImage ();

        }

        private void SetDefaultCorruptNullbody () {

            nullbodyTextureType.SelectedIndex = 1;
            nullbodyBodyColorButton.BackColor = Color.FromArgb (201, 207, 73);
            nullbodyWoundColorButton.BackColor = Color.FromArgb (213, 255, 28);
            nullbodyWireframeColorButton.BackColor = Color.FromArgb (56, 3, 70);
            RefreshNullbodyImage ();

        }
        
        private Bitmap GenerateNullbodyImage (bool corruptTextures, Color wireframeColor, Color bodyColor, Color woundColor) {
            
            Bitmap bmpBg = Properties.Resources.grid_background;
            Bitmap bmpBody = Properties.Resources.body;
            Bitmap bmpWires = corruptTextures ? Properties.Resources.wireframe_corrupt : Properties.Resources.wireframe;
            Bitmap bmpWound = Properties.Resources.wound;
            
            Bitmap bmp = new Bitmap (bmpBg);
            Graphics g = Graphics.FromImage (bmp);
            g.DrawImage (Extensions.Multiply (bmpBody,  bodyColor.R,      bodyColor.G,      bodyColor.B),      0, 0, bmpBg.Size.Width, bmpBg.Size.Height);
            g.DrawImage (Extensions.Multiply (bmpWires, wireframeColor.R, wireframeColor.G, wireframeColor.B), 0, 0, bmpBg.Size.Width, bmpBg.Size.Height);
            g.DrawImage (Extensions.Multiply (bmpWound, woundColor.R,     woundColor.G,     woundColor.B),     0, 0, bmpBg.Size.Width, bmpBg.Size.Height);
            g.Dispose ();

            return bmp;

        }

        private void RefreshNullbodyImage () {
            
            nullbodyPicture.Image = GenerateNullbodyImage (nullbodyTextureType.SelectedIndex > 0, nullbodyWireframeColorButton.BackColor, nullbodyBodyColorButton.BackColor, nullbodyWoundColorButton.BackColor);

        }

        private void ExportNullbodyMats (bool corruptedTextures = false) {
            
            bool isCorrupted = nullbodyTextureType.SelectedIndex > 0;

            // Material names
            string coreMatName = !corruptedTextures ? "mat_nullBody_capsuleCore" : "mat_nullBody_capsuleCore_Green";
            string bodyMatName = !corruptedTextures ? "mat_nullBody_bodySuit"    : "mat_nullBody_bodySuit_Black";
            string faceMatName = !corruptedTextures ? "mat_nullBody_face"        : "mat_nullBody_face_Black";
            string handMatName = !corruptedTextures ? "mat_nullBody_hands"       : "mat_nullBody_hands_Black";
            
            // Texture IDs
            string bodyTexID = !isCorrupted ? "994"  : "698";
            string faceTexID = !isCorrupted ? "1277" : "682";
            string handTexID = !isCorrupted ? "1201" : "1165";
            
            // Get colors
            RGBFloat bodyRGB = new RGBFloat (nullbodyBodyColorButton.BackColor);
            RGBFloat wireRGB = new RGBFloat (nullbodyWireframeColorButton.BackColor);
            RGBFloat woundRGB = new RGBFloat (nullbodyWoundColorButton.BackColor);
            
            // Generate core material string
            string coreMatString = MatGenerators.GenerateCapsuleCoreMatString (coreMatName, bodyRGB);

            // Generate wireframe material strings
            string bodyMatString = "", faceMatString = "", handMatString = "";
            if (!isCorrupted) {

                bodyMatString = MatGenerators.GenerateWireframeMatString (bodyMatName, wireRGB, woundRGB, bodyTexID);
                faceMatString = MatGenerators.GenerateWireframeMatString (faceMatName, wireRGB, woundRGB, faceTexID);
                handMatString = MatGenerators.GenerateWireframeMatString (handMatName, wireRGB, woundRGB, handTexID);

            } else {

                bodyMatString = MatGenerators.GenerateCorruptWireframeMatString (bodyMatName, wireRGB, woundRGB, bodyTexID, "1259", "759",  "720");
                faceMatString = MatGenerators.GenerateCorruptWireframeMatString (faceMatName, wireRGB, woundRGB, faceTexID, "0",    "1184", "720");
                handMatString = MatGenerators.GenerateCorruptWireframeMatString (handMatName, wireRGB, woundRGB, handTexID, "0",    "1184", "720");
                
            }

            // Get the directory
            if (!GetOutputDir (!corruptedTextures ? "nullbody" : "nullbody_corrupt", out string dir))
                return;

            // Write to files
            File.WriteAllText (dir + @"\" + coreMatName + ".txt", coreMatString);
            File.WriteAllText (dir + @"\" + bodyMatName + ".txt", bodyMatString);
            File.WriteAllText (dir + @"\" + faceMatName + ".txt", faceMatString);
            File.WriteAllText (dir + @"\" + handMatName + ".txt", handMatString);

            // Open the folder
            System.Diagnostics.Process.Start (dir);
            
        }

        #endregion

        #region Nightvision

        private void SetDefaultNightvision () {

            nightvisionColorButton.BackColor = Color.FromArgb (18, 255, 18);
            nightvisionIntensityTrackbar.Value = 70;
            UpdateNightvisionIntensityValueLabel ();
            RefreshNightvisionImage ();

        }

        private void nightvisionIntensityTrackbar_Scroll (object sender, EventArgs e) {

            UpdateNightvisionIntensityValueLabel ();
            RefreshNightvisionImage ();

        }

        private void UpdateNightvisionIntensityValueLabel () {
            
            string labelText = ((float)nightvisionIntensityTrackbar.Value / 10f).ToString (Extensions.culture);
            if (!labelText.Contains ("."))
                labelText += ".0";

            nightvisionIntensityValueLabel.Text = labelText;

        }

        private void nightvisionColorButton_Click (object sender, EventArgs e) {

            if (ColorButtonClick (ref nightvisionColorButton))
                RefreshNightvisionImage ();

        }

        private void RefreshNightvisionImage () {

            nightvisionPicture.Image = GenerateNightvisionImage (nightvisionColorButton.BackColor);

        }

        private Bitmap GenerateNightvisionImage (Color color) {

            float mult = Math.Max (Math.Min ((nightvisionIntensityTrackbar.Value + 15f) / 85f, 1f), 0f);
            byte R = (byte)Math.Round (color.R * mult);
            byte G = (byte)Math.Round (color.G * mult);
            byte B = (byte)Math.Round (color.B * mult);

            Bitmap bmpBg = Properties.Resources.background;
            Bitmap bmgNVex = Properties.Resources.nightvisionExample;
            
            Bitmap bmp = new Bitmap (bmpBg);
            Graphics g = Graphics.FromImage (bmp);
            g.DrawImage (Extensions.Multiply (bmgNVex, R, G, B), 0, 0, bmpBg.Size.Width, bmpBg.Size.Height);
            g.Dispose ();

            return bmp;

        }

        private void defaultNightvisionButton_Click (object sender, EventArgs e) {

            SetDefaultNightvision ();

        }

        private void nightvisionHelpButton_Click (object sender, EventArgs e) {

            OpenHelp ();

        }

        private void nightvisionExportButton_Click (object sender, EventArgs e) {

            ExportNightvisionMat ();

        }

        private void ExportNightvisionMat () {

            // Create the material
            RGBFloat color = new RGBFloat (nightvisionColorButton.BackColor);
            float intensity = nightvisionIntensityTrackbar.Value / 10f;
            string nightvisionMatString = MatGenerators.GenerateNightvisionMatString ("NightVision", color, intensity);

            // Get the directory
            if (!GetOutputDir ("nightvision", out string dir))
                return;

            // Write to file
            File.WriteAllText (dir + @"\NightVision.txt", nightvisionMatString);

            // Open the folder
            System.Diagnostics.Process.Start (dir);

        }

        #endregion

        #region Red dot

        private class Reticle {

            public Bitmap reticle = null;
            public Bitmap whiteOverlay = null;

            public Reticle (Bitmap reticle, Bitmap whiteOverlay) {
                this.reticle = reticle;
                this.whiteOverlay = whiteOverlay;
            }

            public Bitmap CreateReticle (Color color) {

                if (reticle == null)
                    return null;

                Bitmap colorReticle = Extensions.Multiply (reticle, color.R, color.G, color.B);

                if (whiteOverlay == null)
                    return colorReticle;

                Bitmap bmp = new Bitmap (colorReticle);
                Graphics g = Graphics.FromImage (bmp);
                g.DrawImage (whiteOverlay, 0, 0, bmp.Size.Width, bmp.Size.Height);
                g.Dispose ();
                return bmp;

            }

        }

        private Reticle [] reticleArray = new Reticle [] {
            new Reticle (Properties.Resources.reticleNormalDot, Properties.Resources.reticleNormalDotWhite),
            new Reticle (Properties.Resources.reticleChevron, Properties.Resources.reticleChevronWhite),
            new Reticle (Properties.Resources.reticleTriangle, Properties.Resources.reticleTriangleWhite),
            new Reticle (Properties.Resources.reticleEotech, null),
            new Reticle (Properties.Resources.reticleHolosun, Properties.Resources.reticleHolosunWhite),
            new Reticle (Properties.Resources.reticleACSS, Properties.Resources.reticleACSSWhite),
            new Reticle (Properties.Resources.reticleKobraT, Properties.Resources.reticleKobraTWhite),
            new Reticle (Properties.Resources.reticleUH1, Properties.Resources.reticleUH1White),
            new Reticle (Properties.Resources.reticleTA31, Properties.Resources.reticleTA31White)
        };
        
        private Bitmap GetReticle (Color color, int reticleID) {

            if (reticleArray.Length == 0)
                return null;

            return reticleArray [Math.Min (reticleArray.Length - 1, Math.Max (reticleID, 0))].CreateReticle (color);

        }

        private void SetDefaultRedDot () {

            redDotTextureComboBox.SelectedIndex = 3;
            redDotColorButton.BackColor = Color.FromArgb (0, 255, 33);
            RefreshRedDotImage ();

        }

        private void defaultRedDotButton_Click (object sender, EventArgs e) {

            SetDefaultRedDot ();

        }

        private void redDotColorButton_Click (object sender, EventArgs e) {

            if (ColorButtonClick (ref redDotColorButton))
                RefreshRedDotImage ();

        }

        private void redDotTextureComboBox_SelectedIndexChanged (object sender, EventArgs e) {

            RefreshRedDotImage ();

        }

        private void redDotHelpButton_Click (object sender, EventArgs e) {

            OpenHelp ();

        }

        private void RefreshRedDotImage () {

            redDotPicture.Image = GenerateRedDotImage (redDotColorButton.BackColor);

        }

        private Bitmap GenerateRedDotImage (Color color) {

            Bitmap bmpBg = Properties.Resources.red_dot_example_background;
            Bitmap bmpReticle = GetReticle (color, redDotTextureComboBox.SelectedIndex);

            Bitmap bmp = new Bitmap (bmpBg);
            Graphics g = Graphics.FromImage (bmp);
            g.DrawImage (bmpReticle, 568, 805, 128, 128);
            g.Dispose ();

            return bmp;

        }

        private void exportRedDotButton_Click (object sender, EventArgs e) {

            ExportRedDot ();

        }

        private void ExportRedDot () {

            // Get the directory
            if (!GetOutputDir ("red dot sight", out string dir))
                return;

            // Create the material
            string reflexScopeMatString = MatGenerators.GenerateReflexScopeMatString ("Reflex Scope");

            // Get the texture
            Bitmap reticle = GetReticle (redDotColorButton.BackColor, redDotTextureComboBox.SelectedIndex);
            byte [] reticleB = Extensions.ImageToByte (reticle);

            // Write to file
            File.WriteAllBytes (dir + @"\Isolated_Reticle.png", reticleB);
            File.WriteAllText (dir + @"\Reflex Scope.txt", reflexScopeMatString);

            // Open the folder
            System.Diagnostics.Process.Start (dir);

        }

        #endregion

        #region Ford

        private void fordHelpButton_Click (object sender, EventArgs e) {

            OpenHelp ();

        }

        private void fordHairColorButton_Click (object sender, EventArgs e) {

            if (ColorButtonClick (ref fordHairColorButton))
                RefreshFordImage ();

        }

        private void fordSkinColorButton_Click (object sender, EventArgs e) {

            if (ColorButtonClick (ref fordSkinColorButton))
                RefreshFordImage ();

        }

        private void fordExportButton_Click (object sender, EventArgs e) {

            ExportFordTextures ();

        }

        private void defaultFordButton_Click (object sender, EventArgs e) {

            SetDefaultFord ();

        }

        private void fordSuitColorButton_Click (object sender, EventArgs e) {

            if (ColorButtonClick (ref fordSuitColorButton))
                RefreshFordImage ();

        }

        private void fordGlowColorButton_Click (object sender, EventArgs e) {

            if (ColorButtonClick (ref fordGlowColorButton))
                RefreshFordImage ();

        }

        private void SetDefaultFord () {

            fordHairColorButton.BackColor = Color.FromArgb (51,   43,  34);
            fordSkinColorButton.BackColor = Color.FromArgb (194, 152, 142);
            fordSuitColorButton.BackColor = Color.FromArgb (120, 126, 136);
            fordGlowColorButton.BackColor = Color.FromArgb (255, 187,  15);
            RefreshFordImage ();

        }

        private void RefreshFordImage () {

            fordPicture.Image = GenerateFordImage (fordHairColorButton.BackColor, fordSkinColorButton.BackColor, fordSuitColorButton.BackColor, fordGlowColorButton.BackColor);

        }

        private Bitmap GenerateFordImage (Color haircolor, Color skinColor, Color bodyColor, Color glowColor) {
            
            Bitmap bmpBg = Properties.Resources.grid_background;
            Bitmap bmpEyes = Properties.Resources.ford_eyes;
            Bitmap bmpSkin = Properties.Resources.ford_skin;
            Bitmap bmpHair = Properties.Resources.ford_hair;
            Bitmap bmpBody = Properties.Resources.ford_bodysuit;
            Bitmap bmpGlow = Properties.Resources.ford_glow;

            int offsetX = 35, offsetY = -40;

            Bitmap bmp = new Bitmap (bmpBg);
            Graphics g = Graphics.FromImage (bmp);
            g.DrawImage (bmpEyes, offsetX, offsetY, bmpBg.Size.Width, bmpBg.Size.Height);
            g.DrawImage (Extensions.Multiply (bmpSkin, skinColor.R, skinColor.G, skinColor.B), offsetX, offsetY, bmpBg.Size.Width, bmpBg.Size.Height);
            g.DrawImage (Extensions.Multiply (bmpHair, haircolor.R, haircolor.G, haircolor.B), offsetX, offsetY, bmpBg.Size.Width, bmpBg.Size.Height);
            g.DrawImage (Extensions.Multiply (bmpBody, bodyColor.R, bodyColor.G, bodyColor.B), offsetX, offsetY, bmpBg.Size.Width, bmpBg.Size.Height);
            g.DrawImage (Extensions.Multiply (bmpGlow, glowColor.R, glowColor.G, glowColor.B), offsetX, offsetY, bmpBg.Size.Width, bmpBg.Size.Height);
            g.Dispose ();

            return bmp;


        }
        
        private void ExportFordTextures () {

            // Get color
            RGBFloat skinColor = new RGBFloat (fordSkinColorButton.BackColor);
            RGBFloat hairColor = new RGBFloat (fordHairColorButton.BackColor);
            RGBFloat suitColor = new RGBFloat (fordSuitColorButton.BackColor);
            RGBFloat glowColor = new RGBFloat (fordGlowColorButton.BackColor);

            // Generate textures
            Bitmap bmpArm = TexGenerators.GenerateFordArms (skinColor);
            Bitmap bmpFace = TexGenerators.GenerateFordFace (skinColor);
            Bitmap bmpHairCap = TexGenerators.GenerateFordHairCap (hairColor);
            Bitmap bmpHairCard = TexGenerators.GenerateFordHairCard (hairColor);
            Bitmap bmpSuit = TexGenerators.GenerateFordBodySuit (suitColor, glowColor);
            Bitmap bmpGlow = TexGenerators.GenerateFordFluorescence (glowColor);

            // Create byte arrays
            byte [] armBytes = Extensions.ImageToByte (bmpArm);
            byte [] faceBytes = Extensions.ImageToByte (bmpFace);
            byte [] hairCapBytes = Extensions.ImageToByte (bmpHairCap);
            byte [] hairCardBytes = Extensions.ImageToByte (bmpHairCard);
            byte [] suitBytes = Extensions.ImageToByte (bmpSuit);
            byte [] glowBytes = Extensions.ImageToByte (bmpGlow);

            // Get the directory
            if (!GetOutputDir ("ford", out string dir))
                return;

            // Write to file
            File.WriteAllBytes (dir + @"\texture_brett_arm_Albedo.png", armBytes);
            File.WriteAllBytes (dir + @"\texture_brett_face_Albedo.png", faceBytes);
            File.WriteAllBytes (dir + @"\texture_brettFace_shortHair_clean_Albedo.png", faceBytes);
            File.WriteAllBytes (dir + @"\brett_hairCap_albedo_alpha.png", hairCapBytes);
            File.WriteAllBytes (dir + @"\brett_hairCard_albedo_alpha.png", hairCardBytes);
            File.WriteAllBytes (dir + @"\texture_bodySuit_Albedo.png", suitBytes);
            File.WriteAllBytes (dir + @"\texture_bodySuit_fluoresence.png", glowBytes);

            // Open the folder
            System.Diagnostics.Process.Start (dir);
            
        }

        #endregion

        #region Voidify

        private void voidHelp_Click (object sender, EventArgs e) {

            OpenHelp ();

        }

        private void voidDistortionTrackBar_Scroll (object sender, EventArgs e) {

            UpdateVoidDistortionValueLabel ();

        }

        private void exportVoidButton_Click (object sender, EventArgs e) {

            ExportVoidifiedMaterial ();

        }

        private void UpdateVoidDistortionValueLabel () {

            string labelText = ((float)voidDistortionTrackBar.Value / 100f).ToString (Extensions.culture);
            if (!labelText.Contains ("."))
                labelText += ".0";

            voidDistortionAmountLabel.Text = labelText;

        }

        private void ExportVoidifiedMaterial () {

            // Create the material
            string matName = voidMaterialInput.Text;
            string voidifiedMatString = MatGenerators.GenerateVoidifiedMatString (matName, voidDistortionTrackBar.Value / 100f);

            // Get the directory
            if (!GetOutputDir ("voidified materials", out string dir))
                return;

            // Write to file
            File.WriteAllText (dir + @"\" + matName + ".txt", voidifiedMatString);

            // Open the folder
            System.Diagnostics.Process.Start (dir);

        }

        #endregion

        
    }

    public class RGBFloat {

        public float R = 0f;
        public float G = 0f;
        public float B = 0f;

        public string sR { get { return R.ToString (Extensions.culture); } }
        public string sG { get { return G.ToString (Extensions.culture); } }
        public string sB { get { return B.ToString (Extensions.culture); } }

        public byte bR { get { return (byte)Math.Round (R * 255); } }
        public byte bG { get { return (byte)Math.Round (G * 255); } }
        public byte bB { get { return (byte)Math.Round (B * 255); } }

        public RGBFloat (Color color) {
            R = color.R / 255f;
            G = color.G / 255f;
            B = color.B / 255f;
        }

        public RGBFloat (float R, float G, float B) {
            this.R = R;
            this.G = G;
            this.B = B;
        }

    }
    
    public static class Extensions {

        public static CultureInfo culture = new CultureInfo ("en-US");

        public static Bitmap Multiply (this Bitmap bitmap, byte r, byte g, byte b, PixelFormat format = PixelFormat.Format32bppArgb) {
            var size = new Rectangle (0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits (size, ImageLockMode.ReadOnly, format);

            var buffer = new byte [bitmapData.Stride * bitmapData.Height];
            Marshal.Copy (bitmapData.Scan0, buffer, 0, buffer.Length);
            bitmap.UnlockBits (bitmapData);

            byte Calc (byte c1, byte c2) {
                var cr = c1 / 255d * c2 / 255d * 255d;
                return (byte)(cr > 255 ? 255 : cr);
            }

            for (var i = 0; i < buffer.Length; i += 4) {
                buffer [i] = Calc (buffer [i], b);
                buffer [i + 1] = Calc (buffer [i + 1], g);
                buffer [i + 2] = Calc (buffer [i + 2], r);
            }

            var result = new Bitmap (bitmap.Width, bitmap.Height);
            var resultData = result.LockBits (size, ImageLockMode.WriteOnly, format);

            Marshal.Copy (buffer, 0, resultData.Scan0, buffer.Length);
            result.UnlockBits (resultData);

            return result;
        }

        public static bool IsValidPath (string path) {
            string result;
            return TryGetFullPath (path, out result);
        }

        public static bool TryGetFullPath (string path, out string result) {
            result = String.Empty;
            if (String.IsNullOrWhiteSpace (path)) { return false; }
            bool status = false;

            try {
                result = Path.GetFullPath (path);
                status = true;
            } catch (ArgumentException) { } catch (SecurityException) { } catch (NotSupportedException) { } catch (PathTooLongException) { }

            return status;
        }

        public static byte[] ImageToByte (Image img)  {

            using (MemoryStream ms = new MemoryStream ()) {

                img.Save (ms, ImageFormat.Png);
                return ms.ToArray ();

            }

        }

    }

    public static class MatGenerators {

        public static string GenerateCapsuleCoreMatString (string matName, RGBFloat coreColor) {

            string str = "" +
                "0 Material Base\r\n" +
                " 1 string m_Name = \"" + matName + "\"\r\n" +
                " 0 PPtr<Shader> m_Shader\r\n" +
                "  0 int m_FileID = 0\r\n" +
                "  0 SInt64 m_PathID = 2613\r\n" +
                " 1 string m_ShaderKeywords = \"G_BCASTSHADOWS_ON S_RECEIVE_SHADOWS S_SPECULAR_METALLIC _DETAIL_MULX2\"\r\n" +
                " 0 unsigned int m_LightmapFlags = 4\r\n" +
                " 0 bool m_EnableInstancingVariants = true\r\n" +
                " 1 bool m_DoubleSidedGI = false\r\n" +
                " 0 int m_CustomRenderQueue = 2000\r\n" +
                " 0 map stringTagMap\r\n" +
                "  0 Array Array (0 items)\r\n" +
                "   0 int size = 0\r\n" +
                " 0 vector disabledShaderPasses\r\n" +
                "  1 Array Array (0 items)\r\n" +
                "   0 int size = 0\r\n" +
                " 0 UnityPropertySheet m_SavedProperties\r\n" +
                "  0 map m_TexEnvs\r\n" +
                "   0 Array Array (0 items)\r\n" +
                "    0 int size = 0\r\n" +
                "  0 map m_Floats\r\n" +
                "   0 Array Array (52 items)\r\n" +
                "    0 int size = 52\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_AnisotropicRatio\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_AnisotropicRotation\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [2]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BumpScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [3]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorMultiplier\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [4]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Cull\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [5]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Cutoff\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [6]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [7]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailNormalMapScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [8]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DistanceFade\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [9]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DstBlend\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [10]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissionFalloff\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [11]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissiveMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [12]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_FogMultiplier\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [13]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_GlossMapScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [14]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Glossiness\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [15]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_GlossyReflections\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [16]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Metallic\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [17]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Mode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [18]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_NormalToOcclusion\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [19]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrength\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [20]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthDirectDiffuse\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [21]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthDirectSpecular\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [22]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthIndirectDiffuse\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [23]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthIndirectSpecular\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [24]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OffsetFactor\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [25]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OffsetUnits\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [26]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_PackingMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [27]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Parallax\"\r\n" +
                "      0 float second = 0.0199999996\r\n" +
                "    [28]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ParallaxIterations\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [29]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ParallaxOffset\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [30]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SmoothnessTextureChannel\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [31]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecMod\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [32]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecularHighlights\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [33]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecularMode\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [34]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SrcBlend\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [35]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Test\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [36]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_UVSec\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [37]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_VertexMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [38]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ZWrite\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [39]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"__dirty\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [40]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bCastShadows\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [41]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bReceiveShadows\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [42]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bRenderBackfaces\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [43]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bUnlit\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [44]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bWorldAlignedTexture\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [45]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flCubeMapScalar\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [46]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flFresnelExponent\"\r\n" +
                "      0 float second = 5\r\n" +
                "    [47]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flFresnelFalloff\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [48]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceBias\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [49]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceMax\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [50]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceMin\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [51]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "  0 map m_Colors\r\n" +
                "   0 Array Array (14 items)\r\n" +
                "    0 int size = 14\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Absorbance\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0.0999999717\r\n" +
                "       0 float g = 0.24999997\r\n" +
                "       0 float b = 0.5\r\n" +
                "       0 float a = 1\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Color\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = " + coreColor.sR + "\r\n" +
                "       0 float g = " + coreColor.sG + "\r\n" +
                "       0 float b = " + coreColor.sB + "\r\n" +
                "       0 float a = 1\r\n" +
                "    [2]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift1\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [3]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift2\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [4]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift3\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [5]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift4\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [6]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissionColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 1\r\n" +
                "    [7]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_FluorescenceColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 1\r\n" +
                "    [8]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0.199999958\r\n" +
                "       0 float g = 0.199999958\r\n" +
                "       0 float b = 0.199999958\r\n" +
                "       0 float a = 1\r\n" +
                "    [9]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedNormalTangentU\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = -1\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 0\r\n" +
                "    [10]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedNormalTangentV\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 0\r\n" +
                "    [11]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTextureNormal\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 0\r\n" +
                "    [12]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTexturePosition\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 0\r\n" +
                "    [13]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTextureSize\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 0\r\n";


            return str;

        }

        public static string GenerateWireframeMatString (string matName, RGBFloat wireColor, RGBFloat woundColor, string textureID) {

            string str = "" +
                "0 Material Base\r\n" +
                " 1 string m_Name = \"" + matName + "\"\r\n" +
                " 0 PPtr<Shader> m_Shader\r\n" +
                "  0 int m_FileID = 0\r\n" +
                "  0 SInt64 m_PathID = 2586\r\n" +
                " 1 string m_ShaderKeywords = \"S_RECEIVE_SHADOWS S_SPECULAR_METALLIC _ALPHABLEND_ON _DETAIL_MULX2 _EMISSION\"\r\n" +
                " 0 unsigned int m_LightmapFlags = 4\r\n" +
                " 0 bool m_EnableInstancingVariants = true\r\n" +
                " 1 bool m_DoubleSidedGI = false\r\n" +
                " 0 int m_CustomRenderQueue = -1\r\n" +
                " 0 map stringTagMap\r\n" +
                "  0 Array Array (1 items)\r\n" +
                "   0 int size = 1\r\n" +
                "   [0]\r\n" +
                "    0 pair data\r\n" +
                "     1 string first = \"OriginalShader\"\r\n" +
                "     1 string second = \"Standard\"\r\n" +
                " 0 vector disabledShaderPasses\r\n" +
                "  1 Array Array (0 items)\r\n" +
                "   0 int size = 0\r\n" +
                " 0 UnityPropertySheet m_SavedProperties\r\n" +
                "  0 map m_TexEnvs\r\n" +
                "   0 Array Array (2 items)\r\n" +
                "    0 int size = 2\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_MainTex\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = " + textureID + "\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_texcoord\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "  0 map m_Floats\r\n" +
                "   0 Array Array (56 items)\r\n" +
                "    0 int size = 56\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_AnisotropicRatio\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_AnisotropicRotation\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [2]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodyTexScale\"\r\n" +
                "      0 float second = 0.00999999978\r\n" +
                "    [3]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BumpScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [4]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorMultiplier\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [5]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Cull\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [6]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Cutoff\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [7]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [8]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailNormalMapScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [9]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DstBlend\"\r\n" +
                "      0 float second = 10\r\n" +
                "    [10]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissionFalloff\"\r\n" +
                "      0 float second = 0.540000021\r\n" +
                "    [11]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissiveMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [12]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_FogMultiplier\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [13]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_GlossMapScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [14]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Glossiness\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [15]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_GlossyReflections\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [16]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Metallic\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [17]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Mode\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [18]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_NormalToOcclusion\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [19]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_NumberOfElipsoids\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [20]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_NumberOfHits\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [21]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrength\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [22]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthDirectDiffuse\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [23]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthDirectSpecular\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [24]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthIndirectDiffuse\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [25]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthIndirectSpecular\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [26]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OffsetFactor\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [27]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OffsetUnits\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [28]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_PackingMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [29]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Parallax\"\r\n" +
                "      0 float second = 0.0199999996\r\n" +
                "    [30]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ParallaxIterations\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [31]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ParallaxOffset\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [32]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Power\"\r\n" +
                "      0 float second = 0.25\r\n" +
                "    [33]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ScanlineDistance\"\r\n" +
                "      0 float second = 20\r\n" +
                "    [34]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ScanlineTime\"\r\n" +
                "      0 float second = 7\r\n" +
                "    [35]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SmoothnessTextureChannel\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [36]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecMod\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [37]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecularHighlights\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [38]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecularMode\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [39]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SrcBlend\"\r\n" +
                "      0 float second = 5\r\n" +
                "    [40]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Test\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [41]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_UVSec\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [42]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_VertexMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [43]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ZWrite\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [44]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bCastShadows\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [45]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bReceiveShadows\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [46]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bRenderBackfaces\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [47]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bUnlit\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [48]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bWorldAlignedTexture\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [49]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flCubeMapScalar\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [50]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flFresnelExponent\"\r\n" +
                "      0 float second = 5\r\n" +
                "    [51]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flFresnelFalloff\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [52]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceBias\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [53]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceMax\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [54]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceMin\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [55]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "  0 map m_Colors\r\n" +
                "   0 Array Array (15 items)\r\n" +
                "    0 int size = 15\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Absorbance\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0.0999999717\r\n" +
                "       0 float g = 0.24999997\r\n" +
                "       0 float b = 0.5\r\n" +
                "       0 float a = 1\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodyColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = " + woundColor.sR + "\r\n" +
                "       0 float g = " + woundColor.sG + "\r\n" +
                "       0 float b = " + woundColor.sB + "\r\n" +
                "       0 float a = 1\r\n" +
                "    [2]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Color\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = " + wireColor.sR + "\r\n" +
                "       0 float g = " + wireColor.sG + "\r\n" +
                "       0 float b = " + wireColor.sB + "\r\n" +
                "       0 float a = 1\r\n" +
                "    [3]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift1\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [4]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift2\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [5]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift3\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [6]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift4\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [7]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissionColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = " + woundColor.sR + "\r\n" +
                "       0 float g = " + woundColor.sG + "\r\n" +
                "       0 float b = " + woundColor.sB + "\r\n" +
                "       0 float a = 1\r\n" +
                "    [8]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_FluorescenceColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 1\r\n" +
                "    [9]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0.199999958\r\n" +
                "       0 float g = 0.199999958\r\n" +
                "       0 float b = 0.199999958\r\n" +
                "       0 float a = 1\r\n" +
                "    [10]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedNormalTangentU\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = -1\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 0\r\n" +
                "    [11]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedNormalTangentV\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 0\r\n" +
                "    [12]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTextureNormal\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 0\r\n" +
                "    [13]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTexturePosition\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 0\r\n" +
                "    [14]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTextureSize\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 0\r\n";

            return str;

        }

        public static string GenerateCorruptWireframeMatString (string matName, RGBFloat wireColor, RGBFloat woundColor, string textureID, string bloodTextureID, string bumpMapTextureID, string metallGlossTextureID) {

            string str = "" +
                "0 Material Base\r\n" +
                " 1 string m_Name = \"" + matName + "\"\r\n" +
                " 0 PPtr<Shader> m_Shader\r\n" +
                "  0 int m_FileID = 0\r\n" +
                "  0 SInt64 m_PathID = 2592\r\n" +
                " 1 string m_ShaderKeywords = \"S_RECEIVE_SHADOWS S_SPECULAR_METALLIC _ALPHABLEND_ON _DETAIL_MULX2 _EMISSION\"\r\n" +
                " 0 unsigned int m_LightmapFlags = 4\r\n" +
                " 0 bool m_EnableInstancingVariants = true\r\n" +
                " 1 bool m_DoubleSidedGI = false\r\n" +
                " 0 int m_CustomRenderQueue = -1\r\n" +
                " 0 map stringTagMap\r\n" +
                "  0 Array Array (1 items)\r\n" +
                "   0 int size = 1\r\n" +
                "   [0]\r\n" +
                "    0 pair data\r\n" +
                "     1 string first = \"OriginalShader\"\r\n" +
                "     1 string second = \"Standard\"\r\n" +
                " 0 vector disabledShaderPasses\r\n" +
                "  1 Array Array (0 items)\r\n" +
                "   0 int size = 0\r\n" +
                " 0 UnityPropertySheet m_SavedProperties\r\n" +
                "  0 map m_TexEnvs\r\n" +
                "   0 Array Array (10 items)\r\n" +
                "    0 int size = 10\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodyNormal\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodyTex\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = " + bloodTextureID + "\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [2]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BumpMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = " + bumpMapTextureID + "\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [3]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailAlbedoMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [4]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailNormalMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [5]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_FluorescenceMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [6]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_MainTex\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = " + textureID + "\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [7]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_MetallicGlossMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = " + metallGlossTextureID + "\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [8]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_texcoord\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [9]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_tBRDFMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "  0 map m_Floats\r\n" +
                "   0 Array Array (63 items)\r\n" +
                "    0 int size = 63\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"EnableBRDFMAP\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"Fluorescence\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [2]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"ReceiveShadows\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [3]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"S\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [4]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_AnisotropicRatio\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [5]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_AnisotropicRotation\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [6]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodyMetallic\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [7]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodyNormalScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [8]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodySmoothness\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [9]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodyTexScale\"\r\n" +
                "      0 float second = 0.00999999978\r\n" +
                "    [10]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BumpScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [11]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorMultiplier\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [12]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Cull\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [13]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Cutoff\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [14]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [15]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailNormalMapScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [16]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DstBlend\"\r\n" +
                "      0 float second = 10\r\n" +
                "    [17]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissionFalloff\"\r\n" +
                "      0 float second = 0.540000021\r\n" +
                "    [18]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissiveMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [19]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_FogMultiplier\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [20]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_GlossMapScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [21]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Glossiness\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [22]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_GlossyReflections\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [23]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Metallic\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [24]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Mode\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [25]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_NormalToOcclusion\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [26]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_NumberOfElipsoids\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [27]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_NumberOfHits\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [28]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrength\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [29]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthDirectDiffuse\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [30]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthDirectSpecular\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [31]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthIndirectDiffuse\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [32]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthIndirectSpecular\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [33]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OffsetFactor\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [34]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OffsetUnits\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [35]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_PackingMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [36]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Parallax\"\r\n" +
                "      0 float second = 0.0199999996\r\n" +
                "    [37]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ParallaxIterations\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [38]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ParallaxOffset\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [39]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Power\"\r\n" +
                "      0 float second = 0.25\r\n" +
                "    [40]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ScanlineDistance\"\r\n" +
                "      0 float second = 20\r\n" +
                "    [41]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ScanlineTime\"\r\n" +
                "      0 float second = 7\r\n" +
                "    [42]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SmoothnessTextureChannel\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [43]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecMod\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [44]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecularHighlights\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [45]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecularMode\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [46]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SrcBlend\"\r\n" +
                "      0 float second = 5\r\n" +
                "    [47]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Test\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [48]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_UVSec\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [49]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_VertexMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [50]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ZWrite\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [51]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bCastShadows\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [52]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bReceiveShadows\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [53]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bRenderBackfaces\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [54]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bUnlit\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [55]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bWorldAlignedTexture\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [56]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flCubeMapScalar\"\r\n" +
                "      0 float second = 0.746999979\r\n" +
                "    [57]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flFresnelExponent\"\r\n" +
                "      0 float second = 5\r\n" +
                "    [58]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flFresnelFalloff\"\r\n" +
                "      0 float second = 1.75999999\r\n" +
                "    [59]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceBias\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [60]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceMax\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [61]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceMin\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [62]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "  0 map m_Colors\r\n" +
                "   0 Array Array (16 items)\r\n" +
                "    0 int size = 16\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Absorbance\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0.0999999717\r\n" +
                "       0 float g = 0.24999997\r\n" +
                "       0 float b = 0.5\r\n" +
                "       0 float a = 1\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodyColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = " + woundColor.sR + "\r\n" +
                "       0 float g = " + woundColor.sG + "\r\n" +
                "       0 float b = " + woundColor.sB + "\r\n" +
                "       0 float a = 1\r\n" +
                "    [2]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodyEmission\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = " + woundColor.sR + "\r\n" +
                "       0 float g = " + woundColor.sG + "\r\n" +
                "       0 float b = " + woundColor.sB + "\r\n" +
                "       0 float a = 0\r\n" +
                "    [3]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Color\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = " + wireColor.sR + "\r\n" +
                "       0 float g = " + wireColor.sG + "\r\n" +
                "       0 float b = " + wireColor.sB + "\r\n" +
                "       0 float a = 1\r\n" +
                "    [4]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift1\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [5]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift2\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [6]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift3\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [7]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift4\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [8]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissionColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = " + wireColor.sR + "\r\n" +
                "       0 float g = " + wireColor.sG + "\r\n" +
                "       0 float b = " + wireColor.sB + "\r\n" +
                "       0 float a = 1\r\n" +
                "    [9]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_FluorescenceColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 1\r\n" +
                "    [10]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0.199999958\r\n" +
                "       0 float g = 0.199999958\r\n" +
                "       0 float b = 0.199999958\r\n" +
                "       0 float a = 1\r\n" +
                "    [11]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedNormalTangentU\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = -1\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 0\r\n" +
                "    [12]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedNormalTangentV\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 0\r\n" +
                "    [13]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTextureNormal\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 0\r\n" +
                "    [14]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTexturePosition\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 0\r\n" +
                "    [15]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTextureSize\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 0\r\n";

            return str;

        }

        public static string GenerateNightvisionMatString (string matName, RGBFloat color, float intensity) {

            string rString = (color.R * Math.Max (1f, intensity)).ToString (Extensions.culture);
            string gString = (color.G * Math.Max (1f, intensity)).ToString (Extensions.culture);
            string bString = (color.B * Math.Max (1f, intensity)).ToString (Extensions.culture);

            string str = "0 Material Base\r\n" +
                " 1 string m_Name = \"" + matName + "\"\r\n" +
                " 0 PPtr<Shader> m_Shader\r\n" +
                "  0 int m_FileID = 0\r\n" +
                "  0 SInt64 m_PathID = 2622\r\n" +
                " 1 string m_ShaderKeywords = \"D_CASTSHADOW _ALPHAMOD2X_ON _ALPHAMULTIPLY_ON _EMISSION\"\r\n" +
                " 0 unsigned int m_LightmapFlags = 4\r\n" +
                " 0 bool m_EnableInstancingVariants = true\r\n" +
                " 1 bool m_DoubleSidedGI = false\r\n" +
                " 0 int m_CustomRenderQueue = 4500\r\n" +
                " 0 map stringTagMap\r\n" +
                "  0 Array Array (2 items)\r\n" +
                "   0 int size = 2\r\n" +
                "   [0]\r\n" +
                "    0 pair data\r\n" +
                "     1 string first = \"OriginalShader\"\r\n" +
                "     1 string second = \"Standard\"\r\n" +
                "   [1]\r\n" +
                "    0 pair data\r\n" +
                "     1 string first = \"RenderType\"\r\n" +
                "     1 string second = \"DO_NOT_RENDER\"\r\n" +
                " 0 vector disabledShaderPasses\r\n" +
                "  1 Array Array (0 items)\r\n" +
                "   0 int size = 0\r\n" +
                " 0 UnityPropertySheet m_SavedProperties\r\n" +
                "  0 map m_TexEnvs\r\n" +
                "   0 Array Array (14 items)\r\n" +
                "    0 int size = 14\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BumpMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorMask\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [2]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailAlbedoMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 40\r\n" +
                "        0 float y = 20\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [3]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailMask\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [4]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailNormalMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [5]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissionMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 830\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 40\r\n" +
                "        0 float y = 1.20000005\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = -0.100000001\r\n" +
                "    [6]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_FluorescenceMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [7]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_MainTex\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 40\r\n" +
                "        0 float y = 1.20000005\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = -0.100000001\r\n" +
                "    [8]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_MetallicGlossMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [9]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [10]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ParallaxMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [11]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecGlossMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [12]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_tBRDFMap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [13]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_tOverrideLightmap\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "  0 map m_Floats\r\n" +
                "   0 Array Array (47 items)\r\n" +
                "    0 int size = 47\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_AnisotropicRatio\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_AnisotropicRotation\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [2]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BumpScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [3]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorMultiplier\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [4]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Cull\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [5]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Cutoff\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [6]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailMode\"\r\n" +
                "      0 float second = 4\r\n" +
                "    [7]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailNormalMapScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [8]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DstBlend\"\r\n" +
                "      0 float second = 3\r\n" +
                "    [9]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissionBakedMultipler\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [10]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissionFalloff\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [11]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissiveMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [12]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_FogMultiplier\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [13]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Glossiness\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [14]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Metallic\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [15]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Mode\"\r\n" +
                "      0 float second = 5\r\n" +
                "    [16]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_NormalToOcclusion\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [17]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrength\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [18]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthDirectDiffuse\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [19]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthDirectSpecular\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [20]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthIndirectDiffuse\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [21]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthIndirectSpecular\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [22]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OffsetFactor\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [23]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OffsetUnits\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [24]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_PackingMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [25]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Parallax\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [26]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ParallaxIterations\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [27]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ParallaxOffset\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [28]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecMod\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [29]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecularMode\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [30]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SrcBlend\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [31]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Test\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [32]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_UVSec\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [33]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_VertexMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [34]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ZWrite\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [35]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bCastShadows\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [36]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bReceiveShadows\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [37]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bRenderBackfaces\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [38]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bUnlit\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [39]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bWorldAlignedTexture\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [40]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flCubeMapScalar\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [41]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flFresnelExponent\"\r\n" +
                "      0 float second = 5\r\n" +
                "    [42]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flFresnelFalloff\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [43]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceBias\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [44]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceMax\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [45]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceMin\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [46]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "  0 map m_Colors\r\n" +
                "   0 Array Array (14 items)\r\n" +
                "    0 int size = 14\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Absorbance\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0.0999999717\r\n" +
                "       0 float g = 0.24999997\r\n" +
                "       0 float b = 0.5\r\n" +
                "       0 float a = 1\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Color\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 1\r\n" +
                "    [2]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift1\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [3]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift2\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [4]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift3\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [5]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift4\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [6]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissionColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = " + rString + "\r\n" +
                "       0 float g = " + gString + "\r\n" +
                "       0 float b = " + bString + "\r\n" +
                "       0 float a = 1\r\n" +
                "    [7]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_FluorescenceColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 1\r\n" +
                "    [8]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0.199999958\r\n" +
                "       0 float g = 0.199999958\r\n" +
                "       0 float b = 0.199999958\r\n" +
                "       0 float a = 1\r\n" +
                "    [9]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedNormalTangentU\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = -1\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 0\r\n" +
                "    [10]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedNormalTangentV\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = -0\r\n" +
                "       0 float a = 0\r\n" +
                "    [11]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTextureNormal\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 0\r\n" +
                "    [12]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTexturePosition\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 0\r\n" +
                "    [13]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTextureSize\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0.100000001\r\n" +
                "       0 float g = 0.100000001\r\n" +
                "       0 float b = 0.100000001\r\n" +
                "       0 float a = 0\r\n";

            return str;

        }

        public static string GenerateReflexScopeMatString (string matName) {

            string str = "0 Material Base\r\n" +
                " 1 string m_Name = \"" + matName + "\"\r\n" +
                " 0 PPtr<Shader> m_Shader\r\n" +
                "  0 int m_FileID = 0\r\n" +
                "  0 SInt64 m_PathID = 2633\r\n" +
                " 1 string m_ShaderKeywords = \"\"\r\n" +
                " 0 unsigned int m_LightmapFlags = 4\r\n" +
                " 0 bool m_EnableInstancingVariants = false\r\n" +
                " 1 bool m_DoubleSidedGI = false\r\n" +
                " 0 int m_CustomRenderQueue = -1\r\n" +
                " 0 map stringTagMap\r\n" +
                "  0 Array Array (0 items)\r\n" +
                "   0 int size = 0\r\n" +
                " 0 vector disabledShaderPasses\r\n" +
                "  1 Array Array (0 items)\r\n" +
                "   0 int size = 0\r\n" +
                " 0 UnityPropertySheet m_SavedProperties\r\n" +
                "  0 map m_TexEnvs\r\n" +
                "   0 Array Array (1 items)\r\n" +
                "    0 int size = 1\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_MainTex\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 841\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "  0 map m_Floats\r\n" +
                "   0 Array Array (6 items)\r\n" +
                "    0 int size = 6\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Cutoff\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Glossiness\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [2]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrength\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [3]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Scale\"\r\n" +
                "      0 float second = 15\r\n" +
                "    [4]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_UVSec\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [5]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_inset\"\r\n" +
                "      0 float second = 8000\r\n" +
                "  0 map m_Colors\r\n" +
                "   0 Array Array (1 items)\r\n" +
                "    0 int size = 1\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_TintColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n";

            return str;

        }

        public static string GenerateVoidifiedMatString (string matName, float distortion) {

            string distortionString = distortion.ToString (Extensions.culture);
            string str = "0 Material Base\r\n" +
                " 1 string m_Name = \"" + matName + "\"\r\n" +
                " 0 PPtr<Shader> m_Shader\r\n" +
                "  0 int m_FileID = 0\r\n" +
                "  0 SInt64 m_PathID = 2583\r\n" +
                " 1 string m_ShaderKeywords = \"G_BCASTSHADOWS_ON S_EMISSIVE_MULTI S_OCCLUSION S_RECEIVE_SHADOWS S_SPECULAR_METALLIC S_WORLD_ALIGNED_TEXTURE _BRDFMAP _DETAIL_MULX2 _EMISSION _PARALLAXMAP\"\r\n" +
                " 0 unsigned int m_LightmapFlags = 4\r\n" +
                " 0 bool m_EnableInstancingVariants = true\r\n" +
                " 1 bool m_DoubleSidedGI = false\r\n" +
                " 0 int m_CustomRenderQueue = -1\r\n" +
                " 0 map stringTagMap\r\n" +
                "  0 Array Array (0 items)\r\n" +
                "   0 int size = 0\r\n" +
                " 0 vector disabledShaderPasses\r\n" +
                "  1 Array Array (0 items)\r\n" +
                "   0 int size = 0\r\n" +
                " 0 UnityPropertySheet m_SavedProperties\r\n" +
                "  0 map m_TexEnvs\r\n" +
                "   0 Array Array (5 items)\r\n" +
                "    0 int size = 5\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BRDFLUT\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 1070\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BackgroundTexture\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 865\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [2]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Distortion\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 1031\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "    [3]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Emission\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 865\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "    [4]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_texcoord\"\r\n" +
                "      0 UnityTexEnv second\r\n" +
                "       0 PPtr<Texture> m_Texture\r\n" +
                "        0 int m_FileID = 0\r\n" +
                "        0 SInt64 m_PathID = 0\r\n" +
                "       0 Vector2f m_Scale\r\n" +
                "        0 float x = 1\r\n" +
                "        0 float y = 1\r\n" +
                "       0 Vector2f m_Offset\r\n" +
                "        0 float x = 0\r\n" +
                "        0 float y = 0\r\n" +
                "  0 map m_Floats\r\n" +
                "   0 Array Array (67 items)\r\n" +
                "    0 int size = 67\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"EnableBRDFMAP\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"ReceiveShadows\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [2]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"S\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [3]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_AnisotropicRatio\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [4]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_AnisotropicRotation\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [5]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodyMetallic\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [6]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodyNormalScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [7]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodySmoothness\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [8]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodyTexScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [9]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BumpScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [10]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_CloneTransparency\"\r\n" +
                "      0 float second = 0.472000003\r\n" +
                "    [11]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_CloneTransparencyStart\"\r\n" +
                "      0 float second = 0.74000001\r\n" +
                "    [12]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorMultiplier\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [13]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Cull\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [14]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Cutoff\"\r\n" +
                "      0 float second = 0.234999999\r\n" +
                "    [15]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [16]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DetailNormalMapScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [17]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DistanceFade\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [18]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DistortBackground\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [19]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DistortEmission\"\r\n" +
                "      0 float second = 3.30999994\r\n" +
                "    [20]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_DstBlend\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [21]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissionFalloff\"\r\n" +
                "      0 float second = 1.88999999\r\n" +
                "    [22]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissiveMode\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [23]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_FogMultiplier\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [24]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_GlossMapScale\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [25]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Glossiness\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [26]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_GlossyReflections\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [27]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Metallic\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [28]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Mode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [29]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_NormalToOcclusion\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [30]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_NumberOfElipsoids\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [31]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_NumberOfHits\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [32]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrength\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [33]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthDirectDiffuse\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [34]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthDirectSpecular\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [35]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthIndirectDiffuse\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [36]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OcclusionStrengthIndirectSpecular\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [37]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OffsetFactor\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [38]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OffsetUnits\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [39]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_PackingMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [40]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Parallax\"\r\n" +
                "      0 float second = -1\r\n" +
                "    [41]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ParallaxIterations\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [42]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ParallaxOffset\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [43]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Power\"\r\n" +
                "      0 float second = 0.25\r\n" +
                "    [44]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SmoothnessTextureChannel\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [45]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecMod\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [46]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecularHighlights\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [47]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecularMode\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [48]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SrcBlend\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [49]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Test\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [50]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_TextureScaling\"\r\n" +
                "      0 float second = 1.50999999\r\n" +
                "    [51]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_UVSec\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [52]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_VertexMode\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [53]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ZWrite\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [54]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"__dirty\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [55]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bCastShadows\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [56]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bReceiveShadows\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [57]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bRenderBackfaces\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [58]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bUnlit\"\r\n" +
                "      0 float second = 0\r\n" +
                "    [59]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_bWorldAlignedTexture\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [60]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flCubeMapScalar\"\r\n" +
                "      0 float second = 1\r\n" +
                "    [61]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flFresnelExponent\"\r\n" +
                "      0 float second = 0.5\r\n" +
                "    [62]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flFresnelFalloff\"\r\n" +
                "      0 float second = 2\r\n" +
                "    [63]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceBias\"\r\n" +
                "      0 float second = 0.00300000003\r\n" +
                "    [64]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceMax\"\r\n" +
                "      0 float second = 0.400000006\r\n" +
                "    [65]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceMin\"\r\n" +
                "      0 float second = 0.00300000003\r\n" +
                "    [66]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_flReflectanceScale\"\r\n" +
                "      0 float second = 0.397000015\r\n" +
                "  0 map m_Colors\r\n" +
                "   0 Array Array (16 items)\r\n" +
                "    0 int size = 16\r\n" +
                "    [0]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Absorbance\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0.0999999717\r\n" +
                "       0 float g = 0.24999997\r\n" +
                "       0 float b = 0.5\r\n" +
                "       0 float a = 1\r\n" +
                "    [1]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_BloodyColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 0.56078428\r\n" +
                "    [2]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_Color\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [3]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift1\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [4]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift2\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [5]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift3\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [6]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_ColorShift4\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 1\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 1\r\n" +
                "    [7]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_EmissionColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 5.72393227\r\n" +
                "       0 float g = 7.79090738\r\n" +
                "       0 float b = 15.1843195\r\n" +
                "       0 float a = 1\r\n" +
                "    [8]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_FluorescenceColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 1\r\n" +
                "    [9]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_OffsetDistance\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = " + distortionString + "\r\n" +
                "       0 float g = " + distortionString + "\r\n" +
                "       0 float b = " + distortionString + "\r\n" +
                "       0 float a = 0\r\n" +
                "    [10]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"_SpecColor\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0.103773594\r\n" +
                "       0 float g = 0.0513972975\r\n" +
                "       0 float b = 0.0606401823\r\n" +
                "       0 float a = 1\r\n" +
                "    [11]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedNormalTangentU\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = -1\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 0\r\n" +
                "    [12]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedNormalTangentV\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 1\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = -0\r\n" +
                "       0 float a = 0\r\n" +
                "    [13]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTextureNormal\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 1\r\n" +
                "       0 float a = 0\r\n" +
                "    [14]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTexturePosition\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 0\r\n" +
                "       0 float g = 0\r\n" +
                "       0 float b = 0\r\n" +
                "       0 float a = 0\r\n" +
                "    [15]\r\n" +
                "     0 pair data\r\n" +
                "      1 string first = \"g_vWorldAlignedTextureSize\"\r\n" +
                "      0 ColorRGBA second\r\n" +
                "       0 float r = 5\r\n" +
                "       0 float g = 5\r\n" +
                "       0 float b = 5\r\n" +
                "       0 float a = 0\r\n";

            return str;

        }

    }

    public static class TexGenerators {

        public static Bitmap GenerateFordArms (RGBFloat color) {

            Bitmap bmp = Properties.Resources.armTexture;
            return Extensions.Multiply (bmp, color.bR, color.bG, color.bB);

        }

        public static Bitmap GenerateFordFace (RGBFloat color) {

            Bitmap bmp = Properties.Resources.faceTexture;
            return Extensions.Multiply (bmp, color.bR, color.bG, color.bB);

        }

        public static Bitmap GenerateFordHairCap (RGBFloat color) {

            Bitmap bmp = Properties.Resources.hairCapTexture;
            return Extensions.Multiply (bmp, color.bR, color.bG, color.bB);

        }

        public static Bitmap GenerateFordHairCard (RGBFloat color) {

            Bitmap bmp = Properties.Resources.hairCardTexture;
            return Extensions.Multiply (bmp, color.bR, color.bG, color.bB);

        }
        
        public static Bitmap GenerateFordBodySuit (RGBFloat color, RGBFloat glowColor) {

            Bitmap bmp = Extensions.Multiply (Properties.Resources.bodySuitTexture, color.bR, color.bG, color.bB);
            Graphics g = Graphics.FromImage (bmp);
            g.DrawImage (Extensions.Multiply (Properties.Resources.fluorescenceOverlayTexture, glowColor.bR, glowColor.bG, glowColor.bB), 0, 0, bmp.Size.Width, bmp.Size.Height);
            g.Dispose ();

            return bmp;

        }

        public static Bitmap GenerateFordFluorescence (RGBFloat color) {

            Bitmap bmp = Properties.Resources.bodySuitFluorescenceTexture;
            return Extensions.Multiply (bmp, color.bR, color.bG, color.bB);

        }
        
    }

}
