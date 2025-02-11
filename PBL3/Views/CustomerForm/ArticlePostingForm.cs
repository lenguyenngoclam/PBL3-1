﻿using PBL3.BLL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PBL3.DTO;
namespace PBL3.Views.CustomerForm
{
    public partial class ArticlePostingForm : Form
    {
        private List<string> ImagePathList;
        private List<string> imageFileName;
        public ArticlePostingForm()
        {
            InitializeComponent();
            LoadCBB();
            ImagePathList = new List<string>();
            imageFileName = new List<string>();
        }
        #region ->Load CBB
        public void LoadCBB()
        {
            CBBItem AllDistrict = new CBBItem
            {
                Value = 0,
                Text = "Tất cả quận"
            };
            CBBItem AllWard = new CBBItem
            {
                Value = 0,
                Text = "Tất cả phường"
            };
            cbb_District.Items.Add(AllDistrict);
            var listDistrict = DistrictBLL.Instance.GetAllDistricts();

            foreach (var i in listDistrict)
            {
                cbb_District.Items.Add(new CBBItem
                {
                    Value = i.DistrictID,
                    Text = i.DistrictName.ToString()
                });
            }
            cbb_District.SelectedItem = AllDistrict;
            cbb_Ward.Items.Add(AllWard);
            cbb_Ward.SelectedItem = AllWard;
        }

        private void cbb_District_OnSelectionChangedCommited(object sender, EventArgs e)
        {
            if (((CBBItem)cbb_District.SelectedItem).Value == 0)
            {
                LoadCBB();
            }
            else
            {
                CBBItem AllWard = new CBBItem
                {
                    Value = 0,
                    Text = "Tất cả phường"
                };
                int districtID = ((CBBItem)cbb_District.SelectedItem).Value;
                cbb_Ward.Items.Clear();
                cbb_Ward.Items.Add(AllWard);
                var WardInDistrict = DistrictBLL.Instance.GetWardsInDistrict(districtID);
                foreach (var i in WardInDistrict)
                {
                    cbb_Ward.Items.Add(new CBBItem
                    {
                        Value = i.WardID,
                        Text = i.WardName
                    });
                }
                cbb_Ward.SelectedItem = AllWard;
            }
        }
        #endregion
        private void uploadImgBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog opFile = new OpenFileDialog();
            opFile.Title = "Chọn ba ảnh";
            opFile.Multiselect = true;
            opFile.Filter = "JPG|*.jpg|JPEG|*.jpeg|GIF|*.gif|PNG|*.png";
            if (opFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (opFile.FileNames.Length != 3)
                    {
                        MessageBox.Show("Bạn phải chọn 3 bức hình!");
                        opFile.Dispose();
                        return;
                    }
                    else if (opFile.FileNames.Distinct().Count() != opFile.FileNames.Length)
                    {
                        MessageBox.Show("Tên file ảnh phải khác nhau!");
                        opFile.Dispose();
                    }
                    IEnumerable<string> imagesIterator = opFile.FileNames.Take(3);
                    string[] images = imagesIterator.ToArray();
                    pictureBox1.Image = System.Drawing.Image.FromFile(images[0]);
                    pictureBox2.Image = System.Drawing.Image.FromFile(images[1]);
                    pictureBox3.Image = System.Drawing.Image.FromFile(images[2]);
                    for (int i = 0; i < 3; i++)
                    {
                        ImagePathList.Add(images[i]);
                        imageFileName.Add(opFile.SafeFileNames[i]);
                    }
                }
                catch (Exception exp)
                {
                    MessageBox.Show("Unable to open file " + exp.Message);
                }
            }
            else
            {
                opFile.Dispose();
            }
        }
        #region ->Validate
        public bool CheckEmpty()
        {
            if (cbb_Ward.SelectedIndex == 0 || DetailAddressTextBox.Texts == "" || titleTextbox.Texts == "" || priceTextBox.Texts == "" ||
                areaTextbox.Texts == "" || descTextbox.Texts == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ các thông tin!");
                return true;
            }
            return false;
        }
        public bool CheckFailImage()
        {
            if (ImagePathList.Count == 0)
            {
                MessageBox.Show("Bạn phải chọn ảnh!");
                return true;
            }
            return false;
        }
        public bool CheckValidPrice()
        {
            int x;
            bool check = int.TryParse(priceTextBox.Texts, out x);
            if (!check)
            {
                MessageBox.Show("Vui lòng nhập giá tiền là một số nguyên!");
                return true;
            }
            if (x <= 0)
            {
                MessageBox.Show("Vui lòng nhập giá tiền là một số nguyên dương!");
                return true;
            }
            return false;
        }
        public bool CheckValidArea()
        {
            double x;
            bool check = double.TryParse(areaTextbox.Texts, out x);
            if (!check)
            {
                MessageBox.Show("Vui lòng nhập diện tích là một số!");
                return true;
            }
            if (x <= 0)
            {
                MessageBox.Show("Vui lòng nhập diện tích là một số dương!");
                return true;
            }
            return false;
        }
        #endregion
        private void uploadArticleBtn_Click(object sender, EventArgs e)
        {
            //validate
            if (CheckFailImage()) return;
            if (CheckEmpty()) return;
            if (CheckValidPrice()) return;
            if (CheckValidArea()) return;
            //Thêm address
            Address temp = new Address
            {
                DetailAddress = DetailAddressTextBox.Texts,
                WardID = ((CBBItem)cbb_Ward.SelectedItem).Value
            };
            int addressID = AddressBLL.Instance.AddAddress(temp);
            //Thêm post
            Post post = new Post()
            {
                UserID = LoginInfo.UserID,
                AddressID = addressID,
                Title = titleTextbox.Texts,
                Description = descTextbox.Texts,
                Price = Convert.ToInt32(priceTextBox.Texts),
                Area = Convert.ToDouble(areaTextbox.Texts),
                BeingPosted = false,
                BeingRented = false,
                CreatedAt = DateTime.Now
            };
            int postID = PostBLL.Instance.AddPost(post);
            //Thêm img
            string imagePathStorage = ImageBLL.Instance.GetImageStoragePathsOfPost(postID);
            if (!Directory.Exists(imagePathStorage))
                Directory.CreateDirectory(imagePathStorage);
            for (int i = 0; i < 3; i++)
            {
                File.Copy(ImagePathList[i], imagePathStorage + @"\" + imageFileName[i]);
                ImageBLL.Instance.AddImage(@"\" + imageFileName[i], postID);
            }

            MessageBox.Show("Bài viết đã được tải lên hệ thống! Admin sẽ xét duyệt bài viết trong thời gian sớm nhất!");
            this.Close();
        }
        private void discardBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
