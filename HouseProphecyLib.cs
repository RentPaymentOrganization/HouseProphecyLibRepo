using System;
using System.Collections.Generic;
using HouseProphecy.Components;
using System.Globalization;
using System.Data;
using System.Web.Script.Serialization;
using HouseProphecy.DataProviders;

namespace HouseProphecy
{
    /// <summary>
    /// Getting a forecast based on user data and information from the database
    /// </summary>
    public class HouseProphecyLib 
    {

        #region public variables

        public ForecastObject Json;
        //public string ConnectionString { get; set; } = string.Empty;

        #endregion

        #region private variables

        private SearchData searchData = new SearchData();
        private ModelData modelData = new ModelData();

        #endregion

        #region public methods

        public HouseProphecyLib(ForecastObject json)
        {
            Json = json;
        }

        /// <summary>
        /// Forecast with aggregation of data
        /// </summary>
        /// <returns>Price result or PredictionException if bad data </returns>
        public double PredictPrice()
        {
            Forecast forecast = new Forecast();
            double price = 0;
            int count = 0;
            string str = string.Empty;
            FillSearchData(Json);
            List<ModelData> modelDataList = new List<ModelData>();
            //collect modelDataList for prediction
            foreach(var catsOk in searchData.CatsOk)
            {
                modelData.CatsOk = catsOk;
                foreach(var dogsOk in searchData.DogsOk)
                {
                    modelData.DogsOk = dogsOk;
                    foreach(var furnished in searchData.Furnished)
                    {
                        modelData.Furnished = furnished;
                        foreach(var noSmoking in searchData.NoSmoking)
                        {
                            modelData.NoSmoking = noSmoking;
                            foreach(var wheelchairAccessible in searchData.WheelchairAccessible)
                            {
                                modelData.WheelchairAccessible = wheelchairAccessible;
                                foreach(var housingType in searchData.HousingType)
                                {
                                    modelData.HousingType = housingType;
                                    foreach(var laundry in searchData.Laundry)
                                    {
                                        modelData.Laundry = laundry;
                                        foreach(var laundrySeparation in searchData.LaundrySeparation)
                                        {
                                            modelData.LaundrySeparation = laundrySeparation;
                                            foreach(var parking in searchData.Parking)
                                            {
                                                modelData.Parking = parking;
                                                modelData.ZipCode = searchData.ZipCode;
                                                modelData.BedRooms = searchData.BedRooms;
                                                modelData.BathRooms = searchData.BathRooms;
                                                modelData.Square = searchData.Square;
                                                modelDataList.Add(modelData);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //do price prediction
            foreach(ModelData modelData in modelDataList)
            {
                forecast.InvokeRequestResponseService(modelData).Wait();
                str = forecast.Result;
                str = str.Substring(str.LastIndexOf(",") + 1);
                str = str.Substring(0, str.IndexOf("]"));
                if(str != Constants.ForecastFields.Nothing)
                {                    
                    str = str.Remove(0, 1);
                    str = str.Remove(str.Length - 1);
                    NumberFormatInfo formatInfo = (NumberFormatInfo)CultureInfo.GetCultureInfo("en-US").NumberFormat.Clone();
                    price += double.Parse(str, formatInfo);
                    count++;
                }
                else
                {
                   throw new PredictionException(HPResources.PredictionFailed) ;
                }
            }
            return Math.Round(price / count, 2);
        }

        /// <summary>
        /// Converting data from datatable to json
        /// </summary>
        /// <param name="table">datatable</param>
        /// <returns>json</returns>
        public string DataTableToJSON(DataTable table)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            foreach(DataRow row in table.Rows)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();

                foreach(DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = (Convert.ToString(row[col]));
                }
                list.Add(dict);
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            return serializer.Serialize(list);
        }

        /// <summary>
        /// Data from the database for presentation on the page
        /// </summary>
        /// <returns>json</returns>
        public string GetPropertyInfoList()
        {
            string resJSON = string.Empty;
            try
            {
                DataSet resDataSet = DataProvider.Instance.GetPropertyListInfo(Json.State, Json.County, Json.City, Json.Street, Json.StreetNumber, Json.ZipCode);
                if(resDataSet.Tables.Count > 0)
                {
                    resJSON = DataTableToJSON(resDataSet.Tables[0]);
                }
            }
            catch(Exception ex)
            {
                resJSON = ex.Message;
            }
            return resJSON;
        }
        public void SetConnectionString(string connectionString)
        {
            DataProvider.Instance.ConnectionString = connectionString;
        }

        #endregion


        #region private methods

        /// <summary>
        /// Data processing from the user to the structure of the model
        /// </summary>
        /// <param name="json">json</param>
        private void FillSearchData(ForecastObject json)
        {
            searchData.ZipCode = json.ZipCode;
            searchData.BedRooms = json.BedRooms;
            searchData.BathRooms = json.BathRooms;
            NumberFormatInfo formatInfo = (NumberFormatInfo)CultureInfo.GetCultureInfo("en-US").NumberFormat.Clone();
            if(json.SquareFrom == null)
            {
                json.SquareFrom = Constants.ForecastFields.SquareFrom;
            }
            if(json.SquareTo == null)
            {
                json.SquareTo = Constants.ForecastFields.SquareTo;
            }
            if(!string.IsNullOrEmpty(json.SquareFrom) && !string.IsNullOrEmpty(json.SquareTo))
            {
                searchData.Square = ((double.Parse(json.SquareFrom, formatInfo) + double.Parse(json.SquareTo, formatInfo)) / 2).ToString();
            }                     
            searchData.CatsOk = ChoiseRadioButton(json.CatsOk);
            searchData.DogsOk = ChoiseRadioButton(json.DogsOk);
            searchData.Furnished = ChoiseRadioButton(json.Furnished);
            searchData.NoSmoking = ChoiseRadioButton(json.NoSmokinkg);
            searchData.WheelchairAccessible = ChoiseRadioButton(json.WheelchairAccessible);
            searchData.HousingType = HousingType(json);
            searchData.Laundry = Laundry(json.Laundry);
            searchData.LaundrySeparation = LaundrySeparation(json.LaundrySeparation);
            searchData.Parking = Parking(json);
        }

        /// <summary>
        /// Data aggregation over the LaundrySeparation field
        /// </summary>
        private List<string> LaundrySeparation(string str)
        {
            List<string> laundrySeparation = new List<string>();
            if(str == Constants.ForecastFields.NoMatter)
            {
                laundrySeparation.Add(Constants.ForecastFields.LaundryInBldg);
                laundrySeparation.Add(Constants.ForecastFields.LaundryOnSite);
                laundrySeparation.Add(Constants.ForecastFields.NoLaundryOnSite);
            }
            else if(str == Constants.ForecastFields.InBldg)
            {
                laundrySeparation.Add(Constants.ForecastFields.LaundryInBldg);
            }
            else if(str == Constants.ForecastFields.OnSite)
            {
                laundrySeparation.Add(Constants.ForecastFields.LaundryOnSite);
            }
            else
            {
                laundrySeparation.Add(Constants.ForecastFields.NoLaundryOnSite);
            }
            return laundrySeparation;
        }

        /// <summary>
        /// Data aggregation over the Laundry field
        /// </summary>
        private List<string> Laundry(string str)
        {
            List<string> laundry = new List<string>();
            if(str == Constants.ForecastFields.NoMatter)
            {
                laundry.Add(Constants.ForecastFields.WDInUnit);
                laundry.Add(Constants.ForecastFields.WDHookups);
            }
            else if(str == Constants.ForecastFields.InUnit)
            {
                laundry.Add(Constants.ForecastFields.WDInUnit);
            }
            else
            {
                laundry.Add(Constants.ForecastFields.WDHookups);
            }
            return laundry;
        }

        /// <summary>
        /// Data aggregation over the Parking field
        /// </summary>
        private List<string> Parking(ForecastObject json)
        {
            List<string> parking = new List<string>();
            if(json.Carport != null)
            {
                parking.Add(Constants.ForecastFields.Carport);
            }
            if(json.AttachedGarage != "")
            {
                parking.Add(Constants.ForecastFields.AttachedGarage);
            }
            if(json.DetachedGarage != "")
            {
                parking.Add(Constants.ForecastFields.DetachedGarage);
            }
            if(json.OffStreetParking != "")
            {
                parking.Add(Constants.ForecastFields.OffStreetParking);
            }
            if(json.StreetParking != "")
            {
                parking.Add(Constants.ForecastFields.StreetParking);
            }
            if(json.ValetParking != "")
            {
                parking.Add(Constants.ForecastFields.ValetParking);
            }
            if(json.NoParking != "")
            {
                parking.Add(Constants.ForecastFields.NoParking);
            }
            if(parking.Count == 0)
            {
                parking.Add(Constants.ForecastFields.Carport);
                parking.Add(Constants.ForecastFields.AttachedGarage);
                parking.Add(Constants.ForecastFields.DetachedGarage);
                parking.Add(Constants.ForecastFields.OffStreetParking);
                parking.Add(Constants.ForecastFields.StreetParking);
                parking.Add(Constants.ForecastFields.ValetParking);
                parking.Add(Constants.ForecastFields.NoParking);
            }
            return parking;
        }

        /// <summary>
        /// Data aggregation over the HousingType field
        /// </summary>
        private List<string> HousingType(ForecastObject json)
        {
            List<string> housingType = new List<string>();
            if(json.Apartment != "")
            {
                housingType.Add(Constants.ForecastFields.Apartment);
            }
            if(json.Condo != "")
            {
                housingType.Add(Constants.ForecastFields.Condo);
            }
            if(json.CottageCabin != "")
            {
                housingType.Add(Constants.ForecastFields.CottageCabin);
            }
            if(json.Duplex != "")
            {
                housingType.Add(Constants.ForecastFields.Duplex);
            }
            if(json.Flat != "")
            {
                housingType.Add(Constants.ForecastFields.Flat);
            }
            if(json.House != "")
            {
                housingType.Add(Constants.ForecastFields.House);
            }
            if(json.InLaw != "")
            {
                housingType.Add(Constants.ForecastFields.InLaw);
            }
            if(json.Loft != "")
            {
                housingType.Add(Constants.ForecastFields.Loft);
            }
            if(json.Townhouse != "")
            {
                housingType.Add(Constants.ForecastFields.Townhouse);
            }
            if(json.Manufactured != "")
            {
                housingType.Add(Constants.ForecastFields.Manufactured);
            }
            if(json.AssistedLiving != "")
            {
                housingType.Add(Constants.ForecastFields.AssistedLiving);
            }
            if(json.Land != "")
            {
                housingType.Add(Constants.ForecastFields.Land);
            }
            if(housingType.Count == 0)
            {
                housingType.Add(Constants.ForecastFields.Apartment);
                housingType.Add(Constants.ForecastFields.Condo);
                housingType.Add(Constants.ForecastFields.CottageCabin);
                housingType.Add(Constants.ForecastFields.Duplex);
                housingType.Add(Constants.ForecastFields.Flat);
                housingType.Add(Constants.ForecastFields.House);
                housingType.Add(Constants.ForecastFields.InLaw);
                housingType.Add(Constants.ForecastFields.Loft);
                housingType.Add(Constants.ForecastFields.Townhouse);
                housingType.Add(Constants.ForecastFields.Manufactured);
                housingType.Add(Constants.ForecastFields.AssistedLiving);
                housingType.Add(Constants.ForecastFields.Land);
            }
            return housingType;
        }

        /// <summary>
        /// Data aggregation over the ChoiseRadioButton field
        /// </summary>
        private List<string> ChoiseRadioButton(string str)
        {
            List<string> list = new List<string>();
            if(str == Constants.ForecastFields.NoMatter)
            {
                list.Add("1");
                list.Add("0");
            }
            else if(str == Constants.ForecastFields.Yes)
            {
                list.Add("1");
            }
            else
            {
                list.Add("0");
            }
            return list;
        }

        #endregion
    }
}