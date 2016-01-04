import socks
import socket
import urllib2
from bs4 import BeautifulSoup
import re
import os
from cookielib import CookieJar

def prapareWeb():
	socks.setdefaultproxy( socks.PROXY_TYPE_SOCKS5, "127.0.0.1", 12345 )
	socket.socket = socks.socksocket

	cookieEater = CookieJar()
	webOpener = urllib2.build_opener( urllib2.HTTPCookieProcessor( cookieEater ) )

	return webOpener


# Loads content of the web page using SOKS5 proxy
def loadWebPage( address, cookies = '' ):
	data = ''
	try:
		opener = urllib2.build_opener()
		if cookies:
			opener.addheaders.append( ( 'Cookie', cookies ) )

		response = opener.open( address )

		chunk = True
		while chunk:
			chunk = response.read()
			data += chunk
	except IOError:
		print 'can\'t open', address
		return ''

	returnValue = {}
	returnValue['cookies'] = response.info().getheader('Set-Cookie')
	returnValue['data'] = data
	response.close()

	print returnValue['cookies']

	return returnValue

# Makes list of links to pdfs
def extractLinksFromSite( htmlContent ):
	parser = BeautifulSoup( htmlContent, 'html.parser' )
	list = parser.find_all( 'pdf' )

	links = []
	for link in list:
		links.append( re.sub('[\[CDATA\]]', '', link.string ) )

	return links


# Loads content of web page and writes it to specyfied file
def loadSiteToFile( address, fileName, cookies = '' ):
	response = loadWebPage( address, cookies )

	htmlContent = response['data']
	cookies = response['cookies']

	if not htmlContent:
		return False
	else:
		target = open( fileName, 'w' )
		target.write( htmlContent )
		target.close

		return True


def getPDFLinks( links, cookies = '' ):

	pdfLinks = []
	for link in links:
		print "Processing link: [" + link + "]"

		response = loadWebPage( link, cookies )
		newPDF = response['data']
		cookies = response['cookies']

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
	#file = open( 'D:\ProgramyVS\Studia\PDFAnal\Docs\ieee.html', 'r' )
	#htmlContent = file.read()

	HTMLresponse = loadWebPage( 'http://ieeexplore.ieee.org/gateway/ipsSearch.jsp?ctype=Conferences&sortfield=py&sortorder=desc' )
	htmlContent = HTMLresponse['data']
	htmlCookie = HTMLresponse['cookies']

	pdfLinks = []
	links = extractLinksFromSite( htmlContent );

	pdfLinks = getPDFLinks( links, htmlCookie )
	
	for pdf in pdfLinks:
		print "Loading pdf: [" + pdf + "]"

		saveFile = makePDFNameFromLink( pdf )
		if loadSiteToFile( pdf, saveFile, htmlCookie ):
			print "PDF saved as: " + saveFile


	#for link in links:
	#	newPDF = loadWebPage( link )

		#filePostfix = link[ link.find( "arnumber=" ) + 9 : ]

		#targetFile = open( filePath + 'ConferencePDF_' + filePostfix + '.pdf', 'a' )
		#targetFile.write( newPDF )
		#targetFile.close

