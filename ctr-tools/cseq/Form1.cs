﻿using System;
using System.Windows.Forms;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace cseq
{
    public partial class MainForm : Form
    {
        public static bool finalLap = false;
        public CSEQ seq;

        string loadedfile = "";

        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void ReadCSEQ(string fn)
        {
            Log.Clear();
            sequenceBox.Items.Clear();
            trackBox.Items.Clear();

            seq = new CSEQ();

            if (seq.Read(fn, textBox1))
            {
                FillUI(fn);
            }
            else
            {
                MessageBox.Show("Failed to read CTR sequence!");
            }
        }

        private void FillUI(string fn)
        {
            loadedfile = Path.GetFileName(fn);
            this.Text = "CTR CSEQ - " + loadedfile;

            textBox2.Text = seq.header.ToString();

            int i = 0;
            sequenceBox.Items.Clear();

            foreach (Sequence s in seq.sequences)
            {
                sequenceBox.Items.Add("Sequence_" + i.ToString("X2"));
                i++;
            }

            textBox1.Text = Log.Read();

            DataSet ds = new DataSet();
            DataTable samples = new DataTable("samples");

            samples.Columns.Add("insttype", typeof(string));
            samples.Columns.Add("magic1", typeof(byte));
            samples.Columns.Add("velocity", typeof(byte));
            samples.Columns.Add("always0", typeof(short));
            samples.Columns.Add("basepitch", typeof(short));
            samples.Columns.Add("sampleID", typeof(short));
            samples.Columns.Add("unknown_80FF", typeof(string));
            samples.Columns.Add("reverb", typeof(byte));
            samples.Columns.Add("always0_2", typeof(byte));

            foreach (Sample s in seq.longSamples)
                s.ToDataRow(samples);

            foreach (Sample s in seq.shortSamples)
                s.ToDataRow(samples);

            ds.Tables.Add(samples);

            dataGridView1.DataSource = ds.Tables["samples"];

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Crash Team Racing CSEQ (*.cseq)|*.cseq";

            if (ofd.ShowDialog() == DialogResult.OK)
                ReadCSEQ(ofd.FileName);
        }

        private void sequenceBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            trackBox.Items.Clear();

            int i = 0;

            foreach (CTrack c in seq.sequences[sequenceBox.SelectedIndex].tracks)
            {
                trackBox.Items.Add(c.name);
                i++;
            }

            tabControl1.SelectedIndex = 1;
        }

        private void trackBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int x = sequenceBox.SelectedIndex;
            int y = trackBox.SelectedIndex;

            if (x != -1 && y != -1)
                this.textBox1.Text = seq.sequences[x].tracks[y].ToString();

            tabControl1.SelectedIndex = 0;
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void sequenceBox_DoubleClick(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "MIDI File (*.mid)|*.mid";
            sfd.FileName = Path.ChangeExtension(loadedfile, ".mid");

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                seq.sequences[sequenceBox.SelectedIndex].ExportMIDI(sfd.FileName);
            }

        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length > 0)
                ReadCSEQ(files[0]);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void skipBytesForUSDemoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CSEQ.usdemo = skipBytesForUSDemoToolStripMenuItem.Checked;
        }

    }
}
