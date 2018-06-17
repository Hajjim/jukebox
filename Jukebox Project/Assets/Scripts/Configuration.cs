using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[XmlRoot("configuration")]
public class Configuration {
    
    public string folder;

    public int malus;

    public int timefornextvote;

    public int sessiontime;

    public int period;

}
