﻿using System.Diagnostics;
using System.Windows.Forms;

namespace TestApplication
{
  public partial class FormAbout : Form
  {
    public FormAbout()
    {
      InitializeComponent();
    }

    private void linkYoutube_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("https://www.youtube.com/user/Switcherlapp97");
    }

    private void linkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("https://github.com/Switcherlapp97/RubiksCubeSolver");
    }

    private void linkMail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("mailto:switcherlapp.97@gmail.com");
    }

  }
}
