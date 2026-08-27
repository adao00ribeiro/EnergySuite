using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace EtrmService.Application.CceeIntegration.DTOs;

[XmlRoot("cceal")]
public class CcealXmlDto
{
    [XmlArray("contratos")]
    [XmlArrayItem("contrato")]
    public List<CcealContratoDto> Contratos { get; set; } = new();
}

public class CcealContratoDto
{
    [XmlElement("codigo_contrato")]
    public string CodigoContrato { get; set; }

    [XmlElement("agente_comprador")]
    public string AgenteComprador { get; set; }

    [XmlElement("agente_vendedor")]
    public string AgenteVendedor { get; set; }

    [XmlElement("inicio_suprimento")]
    public string InicioSuprimento { get; set; }

    [XmlElement("fim_suprimento")]
    public string FimSuprimento { get; set; }

    [XmlElement("montante_mwmed")]
    public decimal MontanteMwmed { get; set; }
}
