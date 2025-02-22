using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.DirectoryServices;
using System.Security.Principal;
using System.DirectoryServices.AccountManagement;


namespace Sers
{
    public partial class Form1 : Form
    {
       
        public Form1()
        {
            InitializeComponent();

            string ldapPath = GetLdapPath();
            label1.Text = $"{ldapPath}";


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            if (!IsUserAdmin())
            {
                MessageBox.Show("Der aktuelle Benutzer ist kein Mitglied der Gruppe 'Administratoren' oder 'Domänen-Admins'.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button1.Enabled = false;
                label9.Text = "❌ Nein!";
            }
            else
            {
               
            }
        }

        private bool IsUserAdmin()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);

                using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity(context, identity.Name);
                    if (user != null)
                    {
                        return user.IsMemberOf(context, IdentityType.Name, "Administratoren") || user.IsMemberOf(context, IdentityType.Name, "Domänen-Admins");
                        label9.Text = "Ja";
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler bei der Überprüfung der Admin-Rechte: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }





        private string GetLdapPath()
        {
            try
            {
                using (DirectoryEntry rootDSE = new DirectoryEntry("LDAP://RootDSE"))
                {
                    string defaultNamingContext = rootDSE.Properties["defaultNamingContext"].Value.ToString();
                    return $"LDAP://{defaultNamingContext}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;
            string newPassword = textBox2.Text;
            string ldapPath = label1.Text.Replace("LDAP Pfad: ", "");

            try
            {
                using (DirectoryEntry entry = new DirectoryEntry(ldapPath))
                {
                    using (DirectorySearcher search = new DirectorySearcher(entry))
                    {
                        search.Filter = $"(sAMAccountName={username})";
                        search.PropertiesToLoad.Add("cn");
                        SearchResult result = search.FindOne();

                        if (result != null)
                        {
                            using (DirectoryEntry userEntry = result.GetDirectoryEntry())
                            {
                                userEntry.Invoke("SetPassword", new object[] { newPassword });
                                userEntry.Properties["LockOutTime"].Value = 0; // Unlock account
                                userEntry.CommitChanges();
                                MessageBox.Show("Das Kennwort des angegebenen Benutzers wurde erfolgreich geändert.", "Kennwort erfolgreich geändert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Der angegebene Benutzer wurde nicht gefunden.", "Benutzer Nicht Gefunden", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler: {ex.Message}","Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        
    }
}
