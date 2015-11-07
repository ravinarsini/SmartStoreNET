﻿using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.Providers
{
	/// <summary>
	/// Exports XML formatted product data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreProductXml")]
	[FriendlyName("SmartStore XML product export")]
	[IsHidden(true)]
	public class ProductXmlExportProvider : ExportProviderBase
	{
		public static string SystemName
		{
			get { return "Exports.SmartStoreProductXml"; }
		}

		public override ExportEntityType EntityType
		{
			get { return ExportEntityType.Product; }
		}

		public override string FileExtension
		{
			get { return "XML"; }
		}

		public override void Execute(IExportExecuteContext context)
		{
			var settings = new XmlWriterSettings
			{
				Encoding = Encoding.UTF8,
				CheckCharacters = false,
				Indent = true,
				IndentChars = "\t"
			};

			var path = context.FilePath;

			context.Log.Information("Creating file " + path);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			using (var writer = XmlWriter.Create(stream, settings))
			{
				var xmlHelper = new ExportXmlHelper(writer, CultureInfo.InvariantCulture);

				writer.WriteStartDocument();
				writer.WriteStartElement("Products");
				writer.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

				while (context.Abort == ExportAbortion.None && context.Segmenter.ReadNextSegment())
				{
					var segment = context.Segmenter.CurrentSegment;

					foreach (dynamic product in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						try
						{
							xmlHelper.WriteProduct(product, "Product");

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, (int)product.Id);
						}
					}
				}

				writer.WriteEndElement();	// Products
				writer.WriteEndDocument();
			}
		}
	}
}
