using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Subtitle_Synchronizer
{
    class CustomExceptions : System.Exception
    {
       
            public CustomExceptions() : base() { }
            public CustomExceptions(string message) : base (message)
            {
              //  MessageBox.Show(message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
            }
        
    }
}
