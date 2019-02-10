using System;
using System.IO;

public class MyFile
{
    public string path;
    public long size;
    public string name;
    public string md5;
    MyFile()
    {

    }
  public  MyFile(FileInfo  file)
    {
        this.name = file.Name;
        this.path = file.FullName;
        this.size = file.Length;
    }
}
