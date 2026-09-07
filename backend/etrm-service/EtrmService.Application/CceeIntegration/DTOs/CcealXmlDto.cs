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
    public string CodigoContrato { get; set; } = string.Empty;

    [XmlElement("agente_comprador")]
    public string AgenteComprador { get; set; } = string.Empty;

    [XmlElement("agente_vendedor")]
    public string AgenteVendedor { get; set; } = string.Empty;

    [XmlElement("inicio_suprimento")]
    public string InicioSuprimento { get; set; } = string.Empty;

    [XmlElement("fim_suprimento")]
    public string FimSuprimento { get; set; } = string.Empty;

    [XmlElement("montante_mwmed")]
    public decimal MontanteMwmed { get; set; }
}
