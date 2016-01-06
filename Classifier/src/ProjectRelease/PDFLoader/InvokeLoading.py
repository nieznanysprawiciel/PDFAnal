import pdfLoader
import sys

proxyAddress = "127.0.0.1"
proxyPort = 12345

outputPath = sys.argv[1]
numberDocuments = 4

pdfLoader.loadData( outputPath, numberDocuments, proxyAddress, proxyPort )
input("Press Enter to close console...")
