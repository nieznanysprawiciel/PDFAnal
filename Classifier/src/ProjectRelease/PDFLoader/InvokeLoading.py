import pdfLoader
import sys

proxyAddress = "127.0.0.1"
proxyPort = 12345
useProxy = True

outputPath = sys.argv[1]
numberDocuments = 25

pdfLoader.loadData( outputPath, numberDocuments, useProxy, proxyAddress, proxyPort )
input("Press Enter to close console...")
