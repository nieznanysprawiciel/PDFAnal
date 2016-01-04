import socks
import socket
import urllib2
from bs4 import BeautifulSoup
import re
import os


# Loads content of the web page using SOKS5 proxy
def loadWebPage( address ):
	socks.setdefaultproxy( socks.PROXY_TYPE_SOCKS5, "127.0.0.1", 12345 )
	socket.socket = socks.socksocket

	data = ''
	try:
		response = urllib2.urlopen( address )

		chunk = True
		while chunk:
			chunk = response.read()
			data += chunk
		response.close()
	except IOError:
		print 'can\'t open', address
		return ''

	return data

# Makes list of links to pdfs
def extractLinksFromSite( htmlContent ):
	parser = BeautifulSoup( htmlContent, 'html.parser' )
	list = parser.find_all( 'pdf' )

	links = []
	for link in list:
		links.append( re.sub('[\[CDATA\]]', '', link.string ) )

	return links


# Loads content of web page and writes it to specyfied file
def loadSiteToFile( address, fileName ):
	htmlContent = loadWebPage( address )

	if not htmlContent:
		return False
	else:
		target = open( fileName, 'w' )
		target.write( htmlContent )
		target.close

		return True


def getPDFLinks( links ):

	pdfLinks = []
	for link in links:
		print "Processing link: [" + link + "]"

		newPDF = loadWebPage( link )

		print "Page loaded. Looking for pdf links..."

		parser = BeautifulSoup( newPDF, 'html.parser' )
		name = parser.find( "meta", {"name":"citation_pdf_url"} )

		if not name:
			print "Pdfs not found."
		else:
			pdfLink = name['content']
			pdfLinks.append( pdfLink )

			print "Found link: " + pdfLink

	return pdfLinks
		

def makePDFNameFromLink( link ):
	filePath = 'D:\ProgramyVS\Studia\PDFAnal\Docs\pdfs\\'
	if not os.path.isdir( filePath ):
		mkdir( filePath )
	
	filePostfix = link[ link.find( "arnumber=" ) + 9 : ]
	targetFile = filePath + 'ConferencePDF_' + filePostfix + '.pdf'
	return targetFile



# Prototyper
def test():
	file = open( 'D:\ProgramyVS\Studia\PDFAnal\Docs\ieee.html', 'r' )
	htmlContent = file.read()

	pdfLinks = []
	links = extractLinksFromSite( htmlContent );

	pdfLinks = getPDFLinks( links )
	
	for pdf in pdfLinks:
		print "Loading pdf: [" + pdf + "]"

		saveFile = makePDFNameFromLink( pdf )
		if loadSiteToFile( pdf, saveFile ):
			print "PDF saved as: " + saveFile


	#for link in links:
	#	newPDF = loadWebPage( link )

		#filePostfix = link[ link.find( "arnumber=" ) + 9 : ]

		#targetFile = open( filePath + 'ConferencePDF_' + filePostfix + '.pdf', 'a' )
		#targetFile.write( newPDF )
		#targetFile.close

