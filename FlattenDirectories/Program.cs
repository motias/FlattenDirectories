using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlattenDirectories
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Count() != 1)
            {
                Console.WriteLine("usage: FlattenDirectories <dir>");
                return;
            }

            var dir = args[0];
            if(!Directory.Exists(dir))
            {
                Console.WriteLine("Directory not found: " + dir);
                return;
            }

            try
            {
                Console.WriteLine("Flattening directories...");
                FlattenDirectory(dir);
                Console.WriteLine();

                // 移動直後は削除に失敗する可能性があるため、スリープを入れる
                Thread.Sleep(3000);

                Console.WriteLine("Deleteing empty directories...");
                DeleteEmptyDirectory(dir);

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// ディレクトリ中で入れ子になっている深い階層のファイル、サブディレクトリを、階層構造を維持したままルートに移動する
        /// </summary>
        /// <param name="dir">処理対象ディレクトリ</param>
        static void FlattenDirectory(string dir)
        {
            foreach (var subdir in Directory.EnumerateDirectories(dir))
            {
                Console.WriteLine("Dir: " + subdir);

                var validDir = SearchValidDirectory(subdir);
                if (!String.IsNullOrWhiteSpace(validDir) && validDir != subdir)
                {
                    Console.WriteLine("Valid dir found: " + validDir);
                    MoveAll(validDir, subdir);

                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// ディレクトリ中にある、ファイルまたは複数のサブディレクトリが存在する有効な階層を検索する
        /// </summary>
        /// <param name="dir">処理対象ディレクトリ</param>
        /// <returns></returns>
        static string SearchValidDirectory(string dir)
        {
            var files = Directory.EnumerateFiles(dir);
            var dirs = Directory.EnumerateDirectories(dir);
            // ファイルが1つ以上、またはディレクトリが複数存在
            if(files.Count() > 0 || dirs.Count() > 1)
            {
                return dir;
            }
            // ディレクトリが1つだけ存在
            else if(files.Count() == 0 && dirs.Count() == 1)
            {
                return SearchValidDirectory(dirs.First());
            }
            // ファイルもディレクトリも存在しない
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// ディレクトリ内のファイル、サブディレクトリを移動する
        /// </summary>
        /// <param name="srcDir">移動元ディレクトリ</param>
        /// <param name="dstDir">移動先ディレクトリ</param>
        static void MoveAll(string srcDir, string dstDir)
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(srcDir))
            {
                var newEntry = Path.Combine(dstDir, Path.GetFileName(entry));
                if (Directory.Exists(entry))
                {
                    Console.WriteLine(string.Format("Move dir: {0} to {1}", entry, newEntry));
                    Directory.Move(entry, newEntry);
                }
                else
                {
                    Console.WriteLine(string.Format("Move file: {0} to {1}", entry, newEntry));
                    File.Move(entry, newEntry);
                }
            }
        }

        /// <summary>
        /// ファイルが一つも存在しない空のディレクトリを削除する
        /// </summary>
        /// <param name="dir">処理対象ディレクトリ</param>
        static void DeleteEmptyDirectory(string dir)
        {
            foreach (var subdir in Directory.EnumerateDirectories(dir))
            {
                if (Directory.EnumerateFiles(subdir, "*", SearchOption.AllDirectories).Count() == 0)
                {
                    Console.WriteLine("Delete empty dir: " + subdir);
                    Directory.Delete(subdir, true);
                }
            }

            foreach (var subdir in Directory.EnumerateDirectories(dir))
            {
                DeleteEmptyDirectory(subdir);
            }
        }
    }
}
