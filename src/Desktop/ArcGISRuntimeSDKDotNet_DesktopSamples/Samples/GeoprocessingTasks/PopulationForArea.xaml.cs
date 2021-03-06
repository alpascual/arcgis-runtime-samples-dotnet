﻿using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample uses a geoprocessing task to estimate the number of people within a polygon you draw on the map. Click 'Summarize Population', then freehand a line on the map surrounding an area to analyze. When you release the mouse button, the polygon will auto-complete and you'll see how many people are estimated to live within the polygon boundaries.
    /// </summary>
    /// <title>Population for an Area</title>
	/// <category>Tasks</category>
	/// <subcategory>Geoprocessing</subcategory>
	public partial class PopulationForArea : UserControl
    {
        /// <summary>Construct Populattion for Area sample control</summary>
        public PopulationForArea()
        {
            InitializeComponent();

            mapView.Map.InitialExtent = new Envelope(-13879981, 3490335, -7778090, 6248898);
        }

        // Accept user boundary line and run the Geoprocessing Task to summarize population
        private async void SummarizePopulationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtResult.Visibility = System.Windows.Visibility.Collapsed;
                AreaLayer.Graphics.Clear();

                var boundary = await mapView.Editor.RequestShapeAsync(DrawShape.Freehand) as Polyline;
                var polygon = new Polygon(boundary, mapView.SpatialReference);
                polygon = GeometryEngine.Simplify(polygon) as Polygon;
                AreaLayer.Graphics.Add(new Graphic() { Geometry = polygon });

                progress.Visibility = Visibility.Visible;

                Geoprocessor geoprocessorTask = new Geoprocessor(
                    new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Population_World/GPServer/PopulationSummary"));

                var parameter = new GPInputParameter() { OutSpatialReference = mapView.SpatialReference };
                parameter.GPParameters.Add(new GPFeatureRecordSetLayer("inputPoly", polygon));

                var result = await geoprocessorTask.ExecuteAsync(parameter);

                GPRecordSet rs = result.OutParameters.OfType<GPRecordSet>().FirstOrDefault();
                if (rs != null && rs.FeatureSet.Features != null)
                {
                    int population = Convert.ToInt32(rs.FeatureSet.Features[0].Attributes["SUM"]);
                    txtResult.Visibility = System.Windows.Visibility.Visible;
                    txtResult.Text = string.Format("Area Population: {0:N0}", population);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Geoprocessor service failed: " + ex.Message, "Sample Error");
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
