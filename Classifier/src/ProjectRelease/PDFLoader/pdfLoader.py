import socks
import socket
import urllib2
from bs4 import BeautifulSoup
import re
import os
from cookielib import CookieJar
from pyPdf import PdfFileWriter, PdfFileReader
from StringIO import StringIO

def prapareWeb( address, port, useproxy = True ):
	if useproxy:
		socks.setdefaultproxy( socks.PROXY_TYPE_SOCKS5, address, port )
		socket.socket = socks.socksocket

	cookieEater = CookieJar()
	webOpener = urllib2.build_opener( urllib2.HTTPCookieProcessor( cookieEater ) )

	return webOpener


# Loads content of the web page using SOKS5 proxy
def loadWebPage( address, webOpener ):
	data = ''
	try:
		response = webOpener.open( address )

		chunk = True
		while chunk:
			chunk = response.read()
			data += chunk
	except IOError:
		print 'can\'t open', address
		return ''

	response.close()

	return data

# Converts string to such form, that can be used as valid path.
# from Django framework, template/defaultfilters.py
def slugify( value ):
    """
    Normalizes string, converts to lowercase, removes non-alpha characters,
    and converts spaces to hyphens.
    """
    import unicodedata
    value = unicodedata.normalize('NFKD', value).encode('ascii', 'ignore')
    value = unicode(re.sub('[^\w\s-]', '', value).strip())
    return re.sub('[-\s]+', '-', value)


# Makes list of links to pdfs
def extractLinksFromPage( htmlContent ):
	parser = BeautifulSoup( htmlContent, 'html.parser' )
	list = parser.find_all( 'pdf' )

	links = []
	for link in list:
		links.append( re.sub('[\[CDATA\]]', '', link.string ) )

	return links


# Makes list of direct links to pdfs and extracts their names
def extractLinksAndNamesFromPage( htmlContent ):
	parser = BeautifulSoup( htmlContent, 'html.parser' )

	documents = parser.find_all('document')

	pdfList = []
	
	for document in documents:
		pdfLink = document.pdf.extract()
		pdfName = document.title.extract()
		pdfAbstract = document.abstract.extract()

		newPdfData = dict()
		newPdfData[ 'link' ] = pdfLink.string
		newPdfData[ 'name' ] = slugify( pdfName.string )
		newPdfData[ 'abstract' ] = pdfAbstract.string
		pdfList.append( newPdfData );

	return pdfList


# Makes list of direct links to pdfs
def extractLinksFromPage2( htmlContent ):
	parser = BeautifulSoup( htmlContent, 'html.parser' )
	frames = parser.frameset.extract()

	while frames:
		found = frames.find_next("frame")

		# There's no link
		if not found:
			return ''
		currentFrame = frames.frame.extract()

		if re.search( '.pdf', currentFrame['src'] ):
			return currentFrame['src']

		

def makePDFNameFromLink( link, filePath ):
	if not os.path.isdir( filePath ):
		mkdir( filePath )
	
	filePostfix = link[ link.find( "arnumber=" ) + 9 : ]
	targetFile = filePath + '\\ConferencePDF_' + filePostfix + '.pdf'
	return targetFile


def loadPDF( address, outputFile, webOpener ):
	writer = PdfFileWriter()

	htmlContent = loadWebPage( address, webOpener )

	if not htmlContent:
		return False

	memoryFile = StringIO( htmlContent )
	pdfFile = PdfFileReader( memoryFile )

	for pageNum in xrange( pdfFile.getNumPages() ):
			currentPage = pdfFile.getPage( pageNum )
			writer.addPage( currentPage )


	outputStream = open( outputFile, "wb" )
	writer.write( outputStream )
	outputStream.close()

	return True


# This is main function for loading PDFs
def loadData( outputDirectory, numberDocuments, useProxy = False, proxyAddress = "127.0.0.1", proxyPort = 12345 ):
	webOpener = prapareWeb( proxyAddress, proxyPort, useProxy )

	maxPortion = 200
	remainingDocs = numberDocuments
	documentOffset = 1

	
	while remainingDocs > 0:
		if remainingDocs < maxPortion:
			maxPortion = remainingDocs
	
		pageName = 'http://ieeexplore.ieee.org/gateway/ipsSearch.jsp?ctype=Conferences&sortfield=py&sortorder=desc' + "&hc=" + str( maxPortion ) + "&rs=" + str( documentOffset )

		print "Loading page: [" + pageName + "]"
		htmlContent = loadWebPage( pageName, webOpener )

		pdfList = extractLinksAndNamesFromPage( htmlContent );
		
		for element in pdfList:

			pageToLoad = element[ 'link' ]
			print "Loading page: [" + pageToLoad + "]"

			pageWithPdfContent = loadWebPage( pageToLoad, webOpener )
			print "Page loaded. Looking for pdf links..."

			directPdfLink = extractLinksFromPage2( pageWithPdfContent )
			print "Found link: " + directPdfLink

			#saveFile = makePDFNameFromLink( directPdfLink, outputDirectory )
			saveFile = outputDirectory + "\\" + element[ 'name' ]
			savePdf = saveFile + ".pdf"
			saveAbstract = saveFile + ".abstract"
			if loadPDF( directPdfLink, savePdf, webOpener ):
				abstractFile = open( saveAbstract, 'w' )
				abstractFile.write( element[ 'abstract' ] )
				abstractFile.close()
				
				print "PDF saved as: " + savePdf
				print "Abstract saved as: " + saveAbstract
		
		remainingDocs = remainingDocs - maxPortion
		documentOffset = documentOffset + maxPortion


#######################################################################################################
###
###							Tests and helper functions, that aren't used no more
###
#######################################################################################################


def testLoadingLinksWithNames():
	webOpener = prapareWeb( "127.0.0.1", 12345 )

	outputDirectory = 'D:\ProgramyVS\Studia\PDFAnal\Docs\pdfs'
	file = open( 'D:\\ProgramyVS\\Studia\\PDFAnal\\Docs\\test\\ipsSearch.jsp.xml', 'r' )
	htmlContent = file.read()

	pdfList = extractLinksAndNamesFromPage( htmlContent );
	
	for element in pdfList:

		pageToLoad = element[ 'link' ]
		print "Loading page: [" + pageToLoad + "]"

		pageWithPdfContent = loadWebPage( pageToLoad, webOpener )
		print "Page loaded. Looking for pdf links..."

		directPdfLink = extractLinksFromPage2( pageWithPdfContent )
		print "Found link: " + directPdfLink

		#saveFile = makePDFNameFromLink( directPdfLink, outputDirectory )
		saveFile = outputDirectory + "\\" + element[ 'name' ] + ".pdf"
		if loadPDF( directPdfLink, saveFile, webOpener ):
			print "PDF saved as: " + saveFile

# Prototyper
def test():
	#file = open( 'D:\ProgramyVS\Studia\PDFAnal\Docs\ieee.html', 'r' )
	#htmlContent = file.read()

	webOpener = prapareWeb( "127.0.0.1", 12345 )

	pageName = 'http://ieeexplore.ieee.org/gateway/ipsSearch.jsp?ctype=Conferences&sortfield=py&sortorder=desc'
	outputFile = 'D:\ProgramyVS\Studia\PDFAnal\Docs\pdfs'

	print "Loading page: [" + pageName + "]"
	htmlContent = loadWebPage( pageName, webOpener )

	links = extractLinksFromPage( htmlContent );

	for pageWithPdf in links:
		print "Loading page: [" + pageWithPdf + "]"

		pageWithPdfContent = loadWebPage( pageWithPdf, webOpener )
		print "Page loaded. Looking for pdf links..."

		directPdfLink = extractLinksFromPage2( pageWithPdfContent )
		print "Found link: " + directPdfLink

		saveFile = makePDFNameFromLink( directPdfLink, outputFile )
		if loadPDF( directPdfLink, saveFile, webOpener ):
			print "PDF saved as: " + saveFile




def writeToFile( content, fileName ):
	target = open( fileName, 'w' )
	target.write( content )
	target.close

# Loads content of web page and writes it to specyfied file
def loadSiteToFile( address, fileName, webOpener ):
	htmlContent = loadWebPage( address, webOpener )

	if not htmlContent:
		return False
	else:
		writeToFile( htmlContent, fileName )
		return True


def getPDFLinks( links, webOpener ):

	pdfLinks = []
	for link in links:
		print "Processing link: [" + link + "]"

		newPDF = loadWebPage( link, webOpener )

		print "Page loaded. Looking for pdf links..."

		parser = BeautifulSoup( newPDF, 'html.parser' )
		name = parser.find( "meta", {"src":"citation_pdf_url"} )

		if not name:
			print "Pdfs not found."
		else:
			pdfLink = name['content']
			pdfLinks.append( pdfLink )

			print "Found link: " + pdfLink

	return pdfLinks


def testPdf():
	webOpener = prapareWeb( "127.0.0.1", 12345 )
	pageName = 'http://ieeexplore.ieee.org/ielx7/7360670/7365697/07365712.pdf?tp=&arnumber=7365712&isnumber=7365697'
	output = 'D:\\ProgramyVS\\Studia\\PDFAnal\\Docs\\test\\pdf.pdf'

	writer = PdfFileWriter()

	print "Loading page: [" + pageName + "]"
	htmlContent = loadWebPage( pageName, webOpener )

	memoryFile = StringIO( htmlContent )
	pdfFile = PdfFileReader( memoryFile )

	for pageNum in xrange( pdfFile.getNumPages() ):
			currentPage = pdfFile.getPage( pageNum )
			#currentPage.mergePage(watermark.getPage(0))
			writer.addPage( currentPage )


	outputStream = open( output, "wb" )
	writer.write( outputStream )
	outputStream.close()

def testLoad():
	webOpener = prapareWeb( "127.0.0.1", 12345 )
	pageName = 'http://ieeexplore.ieee.org/stamp/stamp.jsp?arnumber=7328915'
	output = 'D:\\ProgramyVS\\Studia\\PDFAnal\\Docs\\test\\pdf.txt'

	htmlContent = loadWebPage( pageName, webOpener )

	writeToFile( htmlContent, output )


def testExtractFrame():
	input = 'D:\\ProgramyVS\\Studia\\PDFAnal\\Docs\\test\\pdf.txt'
		
	file = open( input, 'r' )
	htmlContent = file.read()

	parser = BeautifulSoup( htmlContent, 'html.parser' )
	
	frames = parser.frameset.extract()


	while frames:
		found = frames.find_next("frame")

		# There's no link
		if not found:
			return ''
		currentFrame = frames.frame.extract()

		if re.search( '.pdf', currentFrame['src'] ):
			return currentFrame['src']

