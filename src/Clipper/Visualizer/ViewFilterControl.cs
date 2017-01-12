﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Clipper;
using ClipperLib;
using UnitTests;
using IntPoint = ClipperLib.IntPoint;

namespace Visualizer
{
    public partial class ViewFilterControl : UserControl
    {
        private enum SolutionType
        {
            Test,
            NewClipper,
            OriginalClipper
        }

        private Polygon _testSubject;
        private Polygon _testClip;

        private PolygonPath _testSolution;
        private PolygonPath _newClipperSolution;
        private PolygonPath _originalClipperSolution;

        private List<Edge> _testBoundary;
        private List<Edge> _newClipperBoundary;
        private List<Edge> _originalClipperBoundary;

        private SolutionType _solutionType = SolutionType.Test;

        public ViewFilterControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Load unit test data.
            var testData = LoadTestHelper.LoadFromFile("../../../UnitTests/TestData/tests.txt");

            // Get test of interest.
            var test = testData[23];

            _testSubject = test.Subjects.First();
            _testClip = test.Clips.First();
            _testSolution = test.Solution;

            _newClipperSolution = new PolygonPath();

            var clipper = new Clipper.Clipper();

            clipper.AddPaths(test.Subjects, PolygonKind.Subject);
            clipper.AddPaths(test.Clips, PolygonKind.Clip);
            clipper.Execute(ClipOperation.Union, _newClipperSolution);

            _testBoundary = BoundaryBuilder
                .BuildPolygonBoundary(_testSolution.First(), PolygonKind.Subject)
                .ToList();

            _newClipperBoundary = BoundaryBuilder
                .BuildPolygonBoundary(_newClipperSolution.First(), PolygonKind.Subject)
                .ToList();

            var originalClipperSolution = new List<List<IntPoint>>();
            var legacyClipper = new ClipperLib.Clipper();
            legacyClipper.AddPaths(test.Subjects.ToOriginal(), PolyType.ptSubject, true);
            legacyClipper.AddPaths(test.Clips.ToOriginal(), PolyType.ptClip, true);
            legacyClipper.Execute(ClipType.ctUnion, originalClipperSolution);
            _originalClipperSolution = originalClipperSolution.ToNew();

            _originalClipperBoundary = BoundaryBuilder
                .BuildPolygonBoundary(_originalClipperSolution.First(), PolygonKind.Subject)
                .ToList();

            Program.VisualizerForm.ClipperViewControl.Subjects = new[]
            {
                new PolygonViewModel
                {
                    IsOpen = false,
                    EdgeColor = Color.LawnGreen,
                    VertexColor = Color.DarkGreen,
                    Items = _testSubject.ToVertices()
                }
            };

            Program.VisualizerForm.ClipperViewControl.Clips = new[]
            {
                new PolygonViewModel
                {
                    IsOpen = false,
                    EdgeColor = Color.Blue,
                    VertexColor = Color.DarkBlue,
                    Items = _testClip.ToVertices()
                }
            };

            _solutionType = SolutionType.Test;
            SetSolution();
            solutionComboBox.SelectedIndex = 0;
        }

        private void SetSolution()
        {
            Color boundaryEdgeColor;
            Color boundaryVertexColor;
            IReadOnlyList<Edge> boundaryItems;

            Color fillEdgeColor;
            Color fillVertexColor;
            IReadOnlyList<Point> fillItems;

            switch (_solutionType)
            {
                case SolutionType.Test:
                    boundaryEdgeColor = Color.Yellow;
                    boundaryVertexColor = Color.DarkOrange;
                    boundaryItems = viewBoundaryCheckBox.Checked ? _testBoundary : null;

                    fillEdgeColor = Color.FromArgb(60, Color.White);
                    fillVertexColor = Color.White;
                    fillItems = viewFillCheckBox.Checked ? _testSolution.First().ToVertices() : null;
                    break;

                case SolutionType.NewClipper:
                    boundaryEdgeColor = Color.CornflowerBlue;
                    boundaryVertexColor = Color.DarkSlateBlue;
                    boundaryItems = viewBoundaryCheckBox.Checked ? _newClipperBoundary : null;

                    fillEdgeColor = Color.FromArgb(60, Color.SkyBlue);
                    fillVertexColor = Color.SkyBlue;
                    fillItems = viewFillCheckBox.Checked ? _newClipperSolution.First().ToVertices() : null;
                    break;

                case SolutionType.OriginalClipper:
                    boundaryEdgeColor = Color.LawnGreen;
                    boundaryVertexColor = Color.DarkGreen;
                    boundaryItems = viewBoundaryCheckBox.Checked ? _originalClipperBoundary : null;

                    fillEdgeColor = Color.FromArgb(60, Color.LightGreen);
                    fillVertexColor = Color.LightGreen;
                    fillItems = viewFillCheckBox.Checked ? _originalClipperSolution.First().ToVertices() : null;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(_solutionType));
            }


            Program.VisualizerForm.ClipperViewControl.Boundaries = new[]
            {
                new BoundaryViewModel
                {
                    IsOpen = false,
                    EdgeColor = boundaryEdgeColor,
                    VertexColor = boundaryVertexColor,
                    Items = boundaryItems
                }
            };

            Program.VisualizerForm.ClipperViewControl.Fill = new[]
            {
                new PolygonViewModel
                {
                    IsOpen = false,
                    EdgeColor = fillEdgeColor,
                    VertexColor = fillVertexColor,
                    Items = fillItems
                }
            };
        }

        private void ViewSubjectsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Program.VisualizerForm.ClipperViewControl.ViewSubjects = viewSubjectsCheckBox.Checked;
        }

        private void ViewClipsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Program.VisualizerForm.ClipperViewControl.ViewClips = viewClipsCheckBox.Checked;
        }

        private void ViewBoundaryCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetSolution();
        }

        private void ViewFillCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetSolution();
        }

        private void SolutionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Just cast from index.
            _solutionType = (SolutionType) solutionComboBox.SelectedIndex;
            SetSolution();
        }
    }
}