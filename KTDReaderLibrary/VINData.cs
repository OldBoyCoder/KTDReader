using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTDReaderLibrary
{
    public class VINData
    {
        private String vin;
        private String modello;
        private String versione;
        private String serie;
        private String pattern;
        private String internalColor;
        private String guida;
        private String allestimentoMercato;
        private String RTMType;
        public static  VINData NullVINData = new VINData(null, null, null, null, null, null, null, null);

        public VINData(String vin, String modello, String versione, String serie, String guida, String allestimentoMercato, String pattern, String colInt)
        {
            this.vin = vin;
            this.modello = modello;
            this.versione = versione;
            this.serie = serie;
            this.pattern = pattern;
            internalColor = colInt;
            this.guida = guida;
            this.allestimentoMercato = allestimentoMercato;
        }

        public String getRTMType()
        {
            return RTMType;
        }

        public void setRTMType(String rTMType)
        {
            RTMType = rTMType;
        }

        public VINData(String vin, String pattern)
        {
            this.vin = vin;
            this.pattern = pattern;
        }

        public void setVin(String vin)
        {
            this.vin = vin;
        }

        public void setModello(String modello)
        {
            this.modello = modello;
        }

        public void setVersione(String versione)
        {
            this.versione = versione;
        }

        public void setSerie(String serie)
        {
            this.serie = serie;
        }

        public void setPattern(String pattern)
        {
            this.pattern = pattern;
        }

        public void setInternalColor(String internalColor)
        {
            this.internalColor = internalColor;
        }

        public void setGuida(String guida)
        {
            this.guida = guida;
        }

        public void setAllestimentoMercato(String allestimentoMercato)
        {
            this.allestimentoMercato = allestimentoMercato;
        }

        public String getVin()
        {
            return vin;
        }

        public String getModello()
        {
            return modello;
        }

        public String getVersione()
        {
            return versione;
        }

        public String getSerie()
        {
            return serie;
        }

        public String getPattern()
        {
            return pattern;
        }

        public String getInternalColor()
        {
            return internalColor;
        }

        public String getGuida()
        {
            return guida;
        }

        public String getAllestimentoMercato()
        {
            return allestimentoMercato;
        }

        public String getSincom()
        {
            return modello + versione + serie + guida + allestimentoMercato;
        }
    }
}
