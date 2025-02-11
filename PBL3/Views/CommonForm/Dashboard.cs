﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PBL3.BLL;
using PBL3.DTO;
using PBL3.DTO.ViewDTO;
namespace PBL3.Views.CommonForm
{
    public partial class Dashboard : Form
    {
        //Lưu trữ trạng thái của dashboard nếu searching = true thì dashboard đang ở trong trạng thái search
        //Trạng thái sorting sẽ được ưu tiên hơn searching
        //Thứ tự ưu tiên : sorting > searching > default
        private bool searching = false;
        private bool sorting = false;
        //Phần này được sử dụng để hiển thị các bài post
        private int currentPage = 0; //Trang hiện tại của dáhsboard
        private int totalPage; //Tổng số trang của dashboard
        private int numberOfPosts; //Tổng số bài đăng có trên hệ thống và đã được duyệt
        private int skipNum = 5; //1 lần chỉ có thể hiển thị 5 bài post
        private int postNum; //Tổng số bài post ở trong trang hiện tại
        public Dashboard()
        {
            InitializeComponent();
            LoadCBB();
            SearchFunction();
            LoadCBBPageNum();
        }

        #region ->Load CBB
        /*
         * Mặc định ban đầu : Load hết tất cả các quận, Phường chỉ có 1 option Tất cả các phường, 
         * Load hết tất cả các trường
         */
        /*
         * Khi selected item của cbb quận thay đổi thì load phường tương ứng với quận, trường nằm trong phường
         */
        /*
         * Khi selected item của phường thay đổi thì 
         * + Nếu selected item = 0 thì load trường trong quận
         * + Nếu selected item != 0 thì load trường trong phường
         */
        /*
         * Khi selected item của trường đại học thay đổi thì
         * + Nếu selected item của phường là tất cả các phường thì load phường chứa trường đh
         * và quận tương ứng chứa trường đó
         * + Nếu selected item của phường != 0 thì k có gì thay đổi
         */
        public void ResetCBB()
        {
            cbb_University.Items.Clear();
            cbb_University.DataSource = null;
            cbb_University.ResetText();

            cbb_District.Items.Clear();
            cbb_District.DataSource = null;
            cbb_District.ResetText();

            cbb_Ward.Items.Clear();
            cbb_Ward.DataSource = null;
            cbb_Ward.ResetText();
        }
        public void LoadCBB()
        {
            ResetCBB();
            cbb_Sort.SelectedIndex = 0;
            cbb_Price.SelectedIndex = 0;
            cbb_Area.SelectedIndex = 0;
            CBBItem AllDistricts = new CBBItem
            {
                Value = 0,
                Text = "Tất cả quận"
            };
            CBBItem AllWards = new CBBItem
            {
                Value = 0,
                Text = "Tất cả phường"
            };
            CBBItem AllUniversities = new CBBItem
            {
                Value = 0,
                Text = "Tất cả trường"
            };
            cbb_University.Items.Add(AllUniversities);
            //load tất cả trường lên cbb
            var listUni = UniversityBLL.Instance.GetAllUniversities();

            foreach (var i in listUni)
            {
                cbb_University.Items.Add(new CBBItem
                {
                    Value = i.UniversityID,
                    Text = i.UniversityName
                });
            }
            cbb_University.SelectedItem = AllUniversities;
            //load quận
            cbb_District.Items.Add(AllDistricts);
            var listDistrict = DistrictBLL.Instance.GetAllDistricts();

            foreach (var i in listDistrict)
            {
                cbb_District.Items.Add(new CBBItem
                {
                    Value = i.DistrictID,
                    Text = i.DistrictName.ToString()
                });
            }
            cbb_District.SelectedItem = AllDistricts;
            //load và set cbb phường
            cbb_Ward.Items.Add(AllWards);
            cbb_Ward.SelectedItem = AllWards;
        }
        private void cbb_District_OnSelectionChangedCommited(object sender, EventArgs e)
        {
            if (((CBBItem)cbb_District.SelectedItem).Value == 0)
            {
                LoadCBB();
            }
            else
            {
                CBBItem AllWards = new CBBItem
                {
                    Value = 0,
                    Text = "Tất cả phường"
                };
                CBBItem AllUniversities = new CBBItem
                {
                    Value = 0,
                    Text = "Tất cả trường"
                };
                int districtID = ((CBBItem)cbb_District.SelectedItem).Value;

                //load phuong
                cbb_Ward.Items.Clear();
                cbb_Ward.Items.Add(AllWards);
                var WardInDistrict = DistrictBLL.Instance.GetWardsInDistrict(districtID);
                foreach (var i in WardInDistrict)
                {
                    cbb_Ward.Items.Add(new CBBItem
                    {
                        Value = i.WardID,
                        Text = i.WardName
                    });
                }
                cbb_Ward.SelectedItem = AllWards;
                //load truong
                cbb_University.Items.Clear();
                cbb_University.Items.Add(AllUniversities);
                var schoolInDistrict = UniversityBLL.Instance.GetUniInDistrict(districtID);
                foreach (var i in schoolInDistrict)
                {
                    cbb_University.Items.Add(new CBBItem
                    {
                        Value = i.UniversityID,
                        Text = i.UniversityName
                    });
                }
                cbb_University.SelectedItem = AllUniversities;
            }
        }
        private void cbb_Ward_OnSelectionChangedCommited(object sender, EventArgs e)
        {
            CBBItem AllUniversities = new CBBItem
            {
                Value = 0,
                Text = "Tất cả trường"
            };
            int districtID = ((CBBItem)cbb_District.SelectedItem).Value;
            if (((CBBItem)cbb_Ward.SelectedItem).Value != 0)
            {
                int wardID = ((CBBItem)cbb_Ward.SelectedItem).Value;
                cbb_University.Items.Clear();
                cbb_University.Items.Add(AllUniversities);

                var UniInWard = UniversityBLL.Instance.GetUniInWard(wardID);
                foreach (var i in UniInWard)
                {
                    cbb_University.Items.Add(new CBBItem
                    {
                        Value = i.UniversityID,
                        Text = i.UniversityName
                    });
                }
                cbb_University.SelectedItem = AllUniversities;
            }
            else
            {
                cbb_University.Items.Clear();
                cbb_University.Items.Add(AllUniversities);
                var schoolInDistrict = UniversityBLL.Instance.GetUniInDistrict(districtID);
                foreach (var i in schoolInDistrict)
                {
                    cbb_University.Items.Add(new CBBItem
                    {
                        Value = i.UniversityID,
                        Text = i.UniversityName
                    });
                }
                cbb_University.SelectedItem = AllUniversities;
            }
        }
        private void cbb_University_OnSelectionChangedCommited(object sender, EventArgs e)
        {
            if (((CBBItem)cbb_University.SelectedItem).Value != 0)
            {
                CBBItem AllWards = new CBBItem
                {
                    Value = 0,
                    Text = "Tất cả phường"
                };
                int uniID = ((CBBItem)cbb_University.SelectedItem).Value;
                var uni = UniversityBLL.Instance.GetUniversityByUniversityID(uniID);
                //load quận
                int districtID = uni.Ward.District.DistrictID;
                cbb_District.SelectedIndex = districtID;

                //load phuong
                cbb_Ward.Items.Clear();
                cbb_Ward.Items.Add(AllWards);
                var WardInDistrict = DistrictBLL.Instance.GetWardsInDistrict(districtID);
                foreach (var i in WardInDistrict)
                {
                    CBBItem temp = new CBBItem
                    {
                        Value = i.WardID,
                        Text = i.WardName
                    };
                    cbb_Ward.Items.Add(temp);
                    if (temp.Value == uni.Ward.WardID)
                    {
                        cbb_Ward.SelectedItem = temp;
                    }
                }
            }
        }
        #endregion

        #region ->Load CBB Page number
        public void LoadCBBPageNum()
        {
            cbb_PageNumber.Items.Clear();
            cbb_PageNumber.DataSource = null;
            cbb_PageNumber.ResetText();
            int i;
            for (i = 0; i < totalPage; i++)
            {
                CBBItem temp = new CBBItem
                {
                    Value = i,
                    Text = "Trang " + (i + 1).ToString()
                };
                cbb_PageNumber.Items.Add(temp);
                if (temp.Value == currentPage)
                {
                    cbb_PageNumber.SelectedItem = temp;
                }
            }
        }
        private void cbb_PageNumber_OnSelectionChangedCommited(object sender, EventArgs e)
        {
            currentPage = ((CBBItem)cbb_PageNumber.SelectedItem).Value;
            if (currentPage < 0)
                currentPage = totalPage - 1;
            //Sorting được ưu tiên nhất
            if (sorting)
            {
                SortFunction();
                return;
            }
            SearchFunction();
        }
        #endregion

        #region ->Load Dashboard
        private void DisablePostViewWhenNotFound(int postNum)
        {
            //Ẩn các house info component khi số post trên page đó ít hơn 5
            switch (postNum)
            {
                case 4:
                    houseInfoComponent5.Visible = false;
                    break;
                case 3:
                    houseInfoComponent5.Visible = false;
                    houseInfoComponent4.Visible = false;
                    break;
                case 2:
                    houseInfoComponent5.Visible = false;
                    houseInfoComponent4.Visible = false;
                    houseInfoComponent3.Visible = false;
                    break;
                case 1:
                    houseInfoComponent5.Visible = false;
                    houseInfoComponent4.Visible = false;
                    houseInfoComponent3.Visible = false;
                    houseInfoComponent2.Visible = false;
                    break;
                case 0:
                    houseInfoComponent5.Visible = false;
                    houseInfoComponent4.Visible = false;
                    houseInfoComponent3.Visible = false;
                    houseInfoComponent2.Visible = false;
                    houseInfoComponent1.Visible = false;
                    break;
            }
        }
        private void DisplayHouseInformation()
        {
            this.Visible = true;
            panel2.Visible = true;
            houseInfoComponent1.Visible = true;
            houseInfoComponent2.Visible = true;
            houseInfoComponent3.Visible = true;
            houseInfoComponent4.Visible = true;
            houseInfoComponent5.Visible = true;
        }
        private void InitalizeHouseInfomation(List<PostViewDTO> postView)
        {
            //Khởi tạo và hiển thị thông tin lên
            string imagePath;
            if (houseInfoComponent1.Visible)
            {
                houseInfoComponent1.DescLabel = "Miêu tả : " + postView[0].Description;
                houseInfoComponent1.AddressLabel = "Địa chỉ : " + postView[0].Address;
                houseInfoComponent1.HomeLink = postView[0].Title;
                houseInfoComponent1.MoneyLabel = "Số tiền : "
                    + postView[0].Price.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "/Tháng";
                houseInfoComponent1.AreaLabel = "Diện tích : " + postView[0].Area + " m\u00b2";
                houseInfoComponent1.PostID = postView[0].PostID.ToString();

                imagePath = ImageBLL.Instance.GetImageStoragePathsOfPost(postView[0].PostID);
                if (!Directory.Exists(imagePath))
                    Directory.CreateDirectory(imagePath);
                if (postView[0].ImagePaths.Count > 0)
                {
                    System.Drawing.Image image1;
                    using (Stream stream = File.OpenRead(imagePath + postView[0].ImagePaths[0]))
                    {
                        image1 = System.Drawing.Image.FromStream(stream);
                    }
                    houseInfoComponent1.PictureBox = image1;
                }
            }
            if (houseInfoComponent2.Visible)
            {
                houseInfoComponent2.DescLabel = "Miêu tả : " + postView[1].Description;
                houseInfoComponent2.AddressLabel = "Địa chỉ : " + postView[1].Address;
                houseInfoComponent2.HomeLink = postView[1].Title;
                houseInfoComponent2.MoneyLabel = "Số tiền : "
                    + postView[1].Price.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "/Tháng";
                houseInfoComponent2.AreaLabel = "Diện tích : " + postView[1].Area + " m\u00b2";
                houseInfoComponent2.PostID = postView[1].PostID.ToString();

                imagePath = ImageBLL.Instance.GetImageStoragePathsOfPost(postView[1].PostID);
                if (!Directory.Exists(imagePath))
                    Directory.CreateDirectory(imagePath);
                if (postView[1].ImagePaths.Count > 0)
                {
                    System.Drawing.Image image1;
                    using (Stream stream = File.OpenRead(imagePath + postView[1].ImagePaths[0]))
                    {
                        image1 = System.Drawing.Image.FromStream(stream);
                    }
                    houseInfoComponent2.PictureBox = image1;
                }
            }
            if (houseInfoComponent3.Visible)
            {
                houseInfoComponent3.DescLabel = "Miêu tả : " + postView[2].Description;
                houseInfoComponent3.AddressLabel = "Địa chỉ : " + postView[2].Address;
                houseInfoComponent3.HomeLink = postView[2].Title;
                houseInfoComponent3.MoneyLabel = "Số tiền : "
                    + postView[2].Price.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "/Tháng";
                houseInfoComponent3.AreaLabel = "Diện tích : " + postView[2].Area + " m\u00b2";
                houseInfoComponent3.PostID = postView[2].PostID.ToString();

                imagePath = ImageBLL.Instance.GetImageStoragePathsOfPost(postView[2].PostID);
                if (!Directory.Exists(imagePath))
                    Directory.CreateDirectory(imagePath);
                if (postView[2].ImagePaths.Count > 0)
                {
                    System.Drawing.Image image1;
                    using (Stream stream = File.OpenRead(imagePath + postView[2].ImagePaths[0]))
                    {
                        image1 = System.Drawing.Image.FromStream(stream);
                    }
                    houseInfoComponent3.PictureBox = image1;
                }
            }
            if (houseInfoComponent4.Visible)
            {
                houseInfoComponent4.DescLabel = "Miêu tả : " + postView[3].Description;
                houseInfoComponent4.AddressLabel = "Địa chỉ : " + postView[3].Address;
                houseInfoComponent4.HomeLink = postView[3].Title;
                houseInfoComponent4.MoneyLabel = "Số tiền : "
                    + postView[3].Price.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "/Tháng";
                houseInfoComponent4.AreaLabel = "Diện tích : " + postView[3].Area + " m\u00b2";
                houseInfoComponent4.PostID = postView[3].PostID.ToString();

                imagePath = ImageBLL.Instance.GetImageStoragePathsOfPost(postView[3].PostID);
                if (!Directory.Exists(imagePath))
                    Directory.CreateDirectory(imagePath);
                if (postView[3].ImagePaths.Count > 0)
                {
                    System.Drawing.Image image1;
                    using (Stream stream = File.OpenRead(imagePath + postView[3].ImagePaths[0]))
                    {
                        image1 = System.Drawing.Image.FromStream(stream);
                    }
                    houseInfoComponent4.PictureBox = image1;
                }
            }
            if (houseInfoComponent5.Visible)
            {
                houseInfoComponent5.DescLabel = "Miêu tả : " + postView[4].Description;
                houseInfoComponent5.AddressLabel = "Địa chỉ : " + postView[4].Address;
                houseInfoComponent5.HomeLink = postView[4].Title;
                houseInfoComponent5.MoneyLabel = "Số tiền : "
                    + postView[4].Price.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "/Tháng";
                houseInfoComponent5.AreaLabel = "Diện tích : " + postView[4].Area + " m\u00b2";
                houseInfoComponent5.PostID = postView[4].PostID.ToString();

                imagePath = ImageBLL.Instance.GetImageStoragePathsOfPost(postView[4].PostID);
                if (!Directory.Exists(imagePath))
                    Directory.CreateDirectory(imagePath);
                if (postView[4].ImagePaths.Count > 0)
                {
                    System.Drawing.Image image1;
                    using (Stream stream = File.OpenRead(imagePath + postView[4].ImagePaths[0]))
                    {
                        image1 = System.Drawing.Image.FromStream(stream);
                    }
                    houseInfoComponent5.PictureBox = image1;
                }
            }
        }
        private void prevPageBtn_Click(object sender, EventArgs e)
        {
            currentPage = currentPage - 1;
            if (currentPage < 0)
                currentPage = 0;
            LoadCBBPageNum();
            if (sorting)
            {
                SortFunction();
                return;
            }
            SearchFunction();
        }
        private void nextPageBtn_Click(object sender, EventArgs e)
        {
            currentPage = currentPage + 1;
            if (currentPage == totalPage)
            {
                currentPage = totalPage - 1;
                MessageBox.Show("Bạn đã xem hết các bài đăng!");
            }
            LoadCBBPageNum();

            if (sorting)
            {
                SortFunction();
                return;
            }
            SearchFunction();
        }
        #endregion

        #region ->Search post
        public List<Post> GetSearchPost()
        {
            //Search post theo giá trị các cbb địa chỉ, giá, diện tích
            //Mặc định giá trị trái là 0, phải là rất lớn
            int lPrice = 0, rPrice = 999999999;
            float lArea = 0, rArea = 999999999;
            int priceChoice = cbb_Price.SelectedIndex;
            int areaChoice = cbb_Area.SelectedIndex;
            //Set left right cho giá
            switch (priceChoice)
            {
                case 0:
                    lPrice = 0;
                    rPrice = 999999999;
                    break;
                case 1:
                    lPrice = 0;
                    rPrice = 1000000;
                    break;
                case 2:
                    lPrice = 1000000;
                    rPrice = 1500000;
                    break;
                case 3:
                    lPrice = 1500000;
                    rPrice = 2000000;
                    break;
                case 4:
                    lPrice = 2000000;
                    rPrice = 999999999;
                    break;
                default:
                    lPrice = 0;
                    rPrice = 999999999;
                    break;
            }
            //Set left right cho diện tích
            switch (areaChoice)
            {
                case 0:
                    lArea = 0;
                    rArea = 999999999;
                    break;
                case 1:
                    lArea = 0;
                    rArea = 20;
                    break;
                case 2:
                    lArea = 20;
                    rArea = 25;
                    break;
                case 3:
                    lArea = 25;
                    rArea = 30;
                    break;
                case 4:
                    lArea = 30;
                    rArea = 999999999;
                    break;
                default:
                    lArea = 0;
                    rArea = 999999999;
                    break;
            }
            int searchCase = 0, searchID = 0;
            int districtID = ((CBBItem)cbb_District.SelectedItem).Value;
            int wardID = ((CBBItem)cbb_Ward.SelectedItem).Value;
            if (districtID == 0)
            {
                //Case 1: District ID == 0 <=> search trong toàn thành phố
                searchCase = 1;
                searchID = 0;
            }
            else if (wardID == 0)
            {
                //Case 2: District ID != 0 và ward ID == 0  <=> search trong quận
                searchCase = 2;
                searchID = districtID;
            }
            else
            {
                //Case 3: ward ID != 0  <=> search trong phường
                searchCase = 3;
                searchID = wardID;
            }
            if (!searching) searchCase = 100; //không phải đang search nên cho search case rơi vào default
            return PostBLL.Instance.SearchPost(searchCase, searchID, lPrice, rPrice, lArea, rArea);
        }
        private void SearchFunction()
        {
            //Lấy kết quả search
            var allSearchData = GetSearchPost();
            //Lấy và hiển thị search data
            numberOfPosts = allSearchData.Count();
            postNum = (numberOfPosts - currentPage * 5 < 5) ? numberOfPosts - currentPage * 5 : 5;
            totalPage = (int)Math.Ceiling(numberOfPosts / Convert.ToDouble(skipNum));
            DisplayHouseInformation();
            List<PostViewDTO> postView = PostBLL.Instance.GetSearchedPosts(currentPage * skipNum, postNum, allSearchData);
            DisablePostViewWhenNotFound(postNum);
            InitalizeHouseInfomation(postView);
        }
        private void searchBtn_Click(object sender, EventArgs e)
        {
            currentPage = 0;
            searching = true;
            SearchFunction();
            LoadCBBPageNum();
        }
        #endregion

        #region ->Sort post
        private void SortFunction()
        {
            //Lấy dữ liệu hiện tại
            int sortCase = cbb_Sort.SelectedIndex;
            var allSearchData = GetSearchPost();
            //Sort và hiển thị
            numberOfPosts = allSearchData.Count();
            postNum = (numberOfPosts - currentPage * 5 < 5) ? numberOfPosts - currentPage * 5 : 5;
            totalPage = (int)Math.Ceiling(numberOfPosts / Convert.ToDouble(skipNum));
            DisplayHouseInformation();
            List<PostViewDTO> postView = PostBLL.Instance.GetSortedPosts(currentPage * skipNum, postNum, allSearchData, sortCase);
            DisablePostViewWhenNotFound(postNum);
            InitalizeHouseInfomation(postView);
        }
        private void cbb_Sort_OnSelectionChangedCommited(object sender, EventArgs e)
        {
            currentPage = 0;
            sorting = true;
            SortFunction();
            LoadCBBPageNum();
        }
        #endregion

        #region ->Open linked label
        public delegate void showPostDetail(Form childForm);
        public showPostDetail showPost;
        public void ReOpen()
        {
            this.Show();
        }
        private void houseInfoComponent1__OnLabelClicked(object sender, EventArgs e)
        {
            if (LoginInfo.UserID == -1)
            {
                //Chưa đăng nhập => ẩn cmt và rating
                HouseInformationForm form = new HouseInformationForm(Convert.ToInt32(houseInfoComponent1.PostID), true);
                form.goback = ReOpen;
                showPost(form);
            }
            else
            {
                HouseInformationForm form = new HouseInformationForm(Convert.ToInt32(houseInfoComponent1.PostID));
                form.goback = ReOpen;
                showPost(form);
            }
        }
        private void houseInfoComponent2__OnLabelClicked(object sender, EventArgs e)
        {
            if (LoginInfo.UserID == -1)
            {
                //Chưa đăng nhập => ẩn cmt và rating
                HouseInformationForm form = new HouseInformationForm(Convert.ToInt32(houseInfoComponent2.PostID), true);
                form.goback = ReOpen;
                showPost(form);
            }
            else
            {
                HouseInformationForm form = new HouseInformationForm(Convert.ToInt32(houseInfoComponent2.PostID));
                form.goback = ReOpen;
                showPost(form);
            }
        }
        private void houseInfoComponent3__OnLabelClicked(object sender, EventArgs e)
        {
            if (LoginInfo.UserID == -1)
            {
                //Chưa đăng nhập => ẩn cmt và rating
                HouseInformationForm form = new HouseInformationForm(Convert.ToInt32(houseInfoComponent3.PostID), true);
                form.goback = ReOpen;
                showPost(form);
            }
            else
            {
                HouseInformationForm form = new HouseInformationForm(Convert.ToInt32(houseInfoComponent3.PostID));
                form.goback = ReOpen;
                showPost(form);
            }
        }
        private void houseInfoComponent4__OnLabelClicked(object sender, EventArgs e)
        {
            if (LoginInfo.UserID == -1)
            {
                //Chưa đăng nhập => ẩn cmt và rating
                HouseInformationForm form = new HouseInformationForm(Convert.ToInt32(houseInfoComponent4.PostID), true);
                form.goback = ReOpen;
                showPost(form);
            }
            else
            {
                HouseInformationForm form = new HouseInformationForm(Convert.ToInt32(houseInfoComponent4.PostID));
                form.goback = ReOpen;
                showPost(form);
            }
        }
        private void houseInfoComponent5__OnLabelClicked(object sender, EventArgs e)
        {
            if (LoginInfo.UserID == -1)
            {
                //Chưa đăng nhập => ẩn cmt và rating
                HouseInformationForm form = new HouseInformationForm(Convert.ToInt32(houseInfoComponent5.PostID), true);
                form.goback = ReOpen;
                showPost(form);
            }
            else
            {
                HouseInformationForm form = new HouseInformationForm(Convert.ToInt32(houseInfoComponent5.PostID));
                form.goback = ReOpen;
                showPost(form);
            }
        }
        #endregion

        private void resetBtn_Click(object sender, EventArgs e)
        {
            LoadCBB();
            LoadCBBPageNum();
            searching = false;
            sorting = false;
            SearchFunction();
        }
    }
}
