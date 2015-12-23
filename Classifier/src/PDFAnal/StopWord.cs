using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFAnal
{

    class StopWord
    {
        /// <summary>
        /// check whether word is a stop word
        /// </summary>
        public static bool IsStopWord(string word)
        {
            return stopWordDict.ContainsKey(word);
        }
        static Dictionary<string, bool> stopWordDict = new Dictionary<string, bool>
        {
	        { "a", true }, 
	        { "able", true }, 
	        { "about", true }, 
	        { "above", true }, 
	        { "abst", true }, 
	        { "accordance", true }, 
	        { "according", true }, 
	        { "accordingly", true }, 
	        { "across", true }, 
	        { "act", true }, 
	        { "actually", true }, 
	        { "added", true }, 
	        { "adj", true }, 
	        { "affected", true }, 
	        { "affecting", true }, 
	        { "affects", true }, 
	        { "after", true }, 
	        { "afterwards", true }, 
	        { "again", true }, 
	        { "against", true }, 
	        { "ah", true }, 
	        { "all", true }, 
	        { "almost", true }, 
	        { "alone", true }, 
	        { "along", true }, 
	        { "already", true }, 
	        { "also", true }, 
	        { "although", true }, 
	        { "always", true }, 
	        { "am", true }, 
	        { "among", true }, 
	        { "amongst", true }, 
	        { "an", true }, 
	        { "and", true }, 
	        { "announce", true }, 
	        { "another", true }, 
	        { "any", true }, 
	        { "anybody", true }, 
	        { "anyhow", true }, 
	        { "anymore", true }, 
	        { "anyone", true }, 
	        { "anything", true }, 
	        { "anyway", true }, 
	        { "anyways", true }, 
	        { "anywhere", true }, 
	        { "apparently", true }, 
	        { "approximately", true }, 
	        { "are", true }, 
	        { "aren", true }, 
	        { "arent", true }, 
	        { "arise", true }, 
	        { "around", true }, 
	        { "as", true }, 
	        { "aside", true }, 
	        { "ask", true }, 
	        { "asking", true }, 
	        { "at", true }, 
	        { "auth", true }, 
	        { "available", true }, 
	        { "away", true }, 
	        { "awfully", true }, 
	        { "b", true }, 
	        { "back", true }, 
	        { "be", true }, 
	        { "became", true }, 
	        { "because", true }, 
	        { "become", true }, 
	        { "becomes", true }, 
	        { "becoming", true }, 
	        { "been", true }, 
	        { "before", true }, 
	        { "beforehand", true }, 
	        { "begin", true }, 
	        { "beginning", true }, 
	        { "beginnings", true }, 
	        { "begins", true }, 
	        { "behind", true }, 
	        { "being", true }, 
	        { "believe", true }, 
	        { "below", true }, 
	        { "beside", true }, 
	        { "besides", true }, 
	        { "between", true }, 
	        { "beyond", true }, 
	        { "biol", true }, 
	        { "both", true }, 
	        { "brief", true }, 
	        { "briefly", true }, 
	        { "but", true }, 
	        { "by", true }, 
	        { "c", true }, 
	        { "ca", true }, 
	        { "came", true }, 
	        { "can", true }, 
	        { "cannot", true }, 
	        { "can't", true }, 
	        { "cause", true }, 
	        { "causes", true }, 
	        { "certain", true }, 
	        { "certainly", true }, 
	        { "co", true }, 
	        { "com", true }, 
	        { "come", true }, 
	        { "comes", true }, 
	        { "contain", true }, 
	        { "containing", true }, 
	        { "contains", true }, 
	        { "could", true }, 
	        { "couldnt", true }, 
	        { "d", true }, 
	        { "date", true }, 
	        { "did", true }, 
	        { "didn't", true }, 
	        { "different", true }, 
	        { "do", true }, 
	        { "does", true }, 
	        { "doesn't", true }, 
	        { "doing", true }, 
	        { "done", true }, 
	        { "don't", true }, 
	        { "down", true }, 
	        { "downwards", true }, 
	        { "due", true }, 
	        { "during", true }, 
	        { "e", true }, 
	        { "each", true }, 
	        { "ed", true }, 
	        { "edu", true }, 
	        { "effect", true }, 
	        { "eg", true }, 
	        { "eight", true }, 
	        { "eighty", true }, 
	        { "either", true }, 
	        { "else", true }, 
	        { "elsewhere", true }, 
	        { "end", true }, 
	        { "ending", true }, 
	        { "enough", true }, 
	        { "especially", true }, 
	        { "et", true }, 
	        { "et-al", true }, 
	        { "etc", true }, 
	        { "even", true }, 
	        { "ever", true }, 
	        { "every", true }, 
	        { "everybody", true }, 
	        { "everyone", true }, 
	        { "everything", true }, 
	        { "everywhere", true }, 
	        { "ex", true }, 
	        { "except", true }, 
	        { "f", true }, 
	        { "far", true }, 
	        { "few", true }, 
	        { "ff", true }, 
	        { "fifth", true }, 
	        { "first", true }, 
	        { "five", true }, 
	        { "fix", true }, 
	        { "followed", true }, 
	        { "following", true }, 
	        { "follows", true }, 
	        { "for", true }, 
	        { "former", true }, 
	        { "formerly", true }, 
	        { "forth", true }, 
	        { "found", true }, 
	        { "four", true }, 
	        { "from", true }, 
	        { "further", true }, 
	        { "furthermore", true }, 
	        { "g", true }, 
	        { "gave", true }, 
	        { "get", true }, 
	        { "gets", true }, 
	        { "getting", true }, 
	        { "give", true }, 
	        { "given", true }, 
	        { "gives", true }, 
	        { "giving", true }, 
	        { "go", true }, 
	        { "goes", true }, 
	        { "gone", true }, 
	        { "got", true }, 
	        { "gotten", true }, 
	        { "h", true }, 
	        { "had", true }, 
	        { "happens", true }, 
	        { "hardly", true }, 
	        { "has", true }, 
	        { "hasn't", true }, 
	        { "have", true }, 
	        { "haven't", true }, 
	        { "having", true }, 
	        { "he", true }, 
	        { "hed", true }, 
	        { "hence", true }, 
	        { "her", true }, 
	        { "here", true }, 
	        { "hereafter", true }, 
	        { "hereby", true }, 
	        { "herein", true }, 
	        { "heres", true }, 
	        { "hereupon", true }, 
	        { "hers", true }, 
	        { "herself", true }, 
	        { "hes", true }, 
	        { "hi", true }, 
	        { "hid", true }, 
	        { "him", true }, 
	        { "himself", true }, 
	        { "his", true }, 
	        { "hither", true }, 
	        { "home", true }, 
	        { "how", true }, 
	        { "howbeit", true }, 
	        { "however", true }, 
	        { "hundred", true }, 
	        { "i", true }, 
	        { "id", true }, 
	        { "ie", true }, 
	        { "if", true }, 
	        { "i'll", true }, 
	        { "im", true }, 
	        { "immediate", true }, 
	        { "immediately", true }, 
	        { "importance", true }, 
	        { "important", true }, 
	        { "in", true }, 
	        { "inc", true }, 
	        { "indeed", true }, 
	        { "index", true }, 
	        { "information", true }, 
	        { "instead", true }, 
	        { "into", true }, 
	        { "invention", true }, 
	        { "inward", true }, 
	        { "is", true }, 
	        { "isn't", true }, 
	        { "it", true }, 
	        { "itd", true }, 
	        { "it'll", true }, 
	        { "its", true }, 
	        { "itself", true }, 
	        { "i've", true }, 
	        { "j", true }, 
	        { "just", true }, 
	        { "k", true }, 
	        { "keep	keeps", true }, 
	        { "kept", true }, 
	        { "kg", true }, 
	        { "km", true }, 
	        { "know", true }, 
	        { "known", true }, 
	        { "knows", true }, 
	        { "l", true }, 
	        { "largely", true }, 
	        { "last", true }, 
	        { "lately", true }, 
	        { "later", true }, 
	        { "latter", true }, 
	        { "latterly", true }, 
	        { "least", true }, 
	        { "less", true }, 
	        { "lest", true }, 
	        { "let", true }, 
	        { "lets", true }, 
	        { "like", true }, 
	        { "liked", true }, 
	        { "likely", true }, 
	        { "line", true }, 
	        { "little", true }, 
	        { "'ll", true }, 
	        { "look", true }, 
	        { "looking", true }, 
	        { "looks", true }, 
	        { "ltd", true }, 
	        { "m", true }, 
	        { "made", true }, 
	        { "mainly", true }, 
	        { "make", true }, 
	        { "makes", true }, 
	        { "many", true }, 
	        { "may", true }, 
	        { "maybe", true }, 
	        { "me", true }, 
	        { "mean", true }, 
	        { "means", true }, 
	        { "meantime", true }, 
	        { "meanwhile", true }, 
	        { "merely", true }, 
	        { "mg", true }, 
	        { "might", true }, 
	        { "million", true }, 
	        { "miss", true }, 
	        { "ml", true }, 
	        { "more", true }, 
	        { "moreover", true }, 
	        { "most", true }, 
	        { "mostly", true }, 
	        { "mr", true }, 
	        { "mrs", true }, 
	        { "much", true }, 
	        { "mug", true }, 
	        { "must", true }, 
	        { "my", true }, 
	        { "myself", true }, 
	        { "n", true }, 
	        { "na", true }, 
	        { "name", true }, 
	        { "namely", true }, 
	        { "nay", true }, 
	        { "nd", true }, 
	        { "near", true }, 
	        { "nearly", true }, 
	        { "necessarily", true }, 
	        { "necessary", true }, 
	        { "need", true }, 
	        { "needs", true }, 
	        { "neither", true }, 
	        { "never", true }, 
	        { "nevertheless", true }, 
	        { "new", true }, 
	        { "next", true }, 
	        { "nine", true }, 
	        { "ninety", true }, 
	        { "no", true }, 
	        { "nobody", true }, 
	        { "non", true }, 
	        { "none", true }, 
	        { "nonetheless", true }, 
	        { "noone", true }, 
	        { "nor", true }, 
	        { "normally", true }, 
	        { "nos", true }, 
	        { "not", true }, 
	        { "noted", true }, 
	        { "nothing", true }, 
	        { "now", true }, 
	        { "nowhere", true }, 
	        { "o", true }, 
	        { "obtain", true }, 
	        { "obtained", true }, 
	        { "obviously", true }, 
	        { "of", true }, 
	        { "off", true }, 
	        { "often", true }, 
	        { "oh", true }, 
	        { "ok", true }, 
	        { "okay", true }, 
	        { "old", true }, 
	        { "omitted", true }, 
	        { "on", true }, 
	        { "once", true }, 
	        { "one", true }, 
	        { "ones", true }, 
	        { "only", true }, 
	        { "onto", true }, 
	        { "or", true }, 
	        { "ord", true }, 
	        { "other", true }, 
	        { "others", true }, 
	        { "otherwise", true }, 
	        { "ought", true }, 
	        { "our", true }, 
	        { "ours", true }, 
	        { "ourselves", true }, 
	        { "out", true }, 
	        { "outside", true }, 
	        { "over", true }, 
	        { "overall", true }, 
	        { "owing", true }, 
	        { "own", true }, 
	        { "p", true }, 
	        { "page", true }, 
	        { "pages", true }, 
	        { "part", true }, 
	        { "particular", true }, 
	        { "particularly", true }, 
	        { "past", true }, 
	        { "per", true }, 
	        { "perhaps", true }, 
	        { "placed", true }, 
	        { "please", true }, 
	        { "plus", true }, 
	        { "poorly", true }, 
	        { "possible", true }, 
	        { "possibly", true }, 
	        { "potentially", true }, 
	        { "pp", true }, 
	        { "predominantly", true }, 
	        { "present", true }, 
	        { "previously", true }, 
	        { "primarily", true }, 
	        { "probably", true }, 
	        { "promptly", true }, 
	        { "proud", true }, 
	        { "provides", true }, 
	        { "put", true }, 
	        { "q", true }, 
	        { "que", true }, 
	        { "quickly", true }, 
	        { "quite", true }, 
	        { "qv", true }, 
	        { "r", true }, 
	        { "ran", true }, 
	        { "rather", true }, 
	        { "rd", true }, 
	        { "re", true }, 
	        { "readily", true }, 
	        { "really", true }, 
	        { "recent", true }, 
	        { "recently", true }, 
	        { "ref", true }, 
	        { "refs", true }, 
	        { "regarding", true }, 
	        { "regardless", true }, 
	        { "regards", true }, 
	        { "related", true }, 
	        { "relatively", true }, 
	        { "research", true }, 
	        { "respectively", true }, 
	        { "resulted", true }, 
	        { "resulting", true }, 
	        { "results", true }, 
	        { "right", true }, 
	        { "run", true }, 
	        { "s", true }, 
	        { "said", true }, 
	        { "same", true }, 
	        { "saw", true }, 
	        { "say", true }, 
	        { "saying", true }, 
	        { "says", true }, 
	        { "sec", true }, 
	        { "section", true }, 
	        { "see", true }, 
	        { "seeing", true }, 
	        { "seem", true }, 
	        { "seemed", true }, 
	        { "seeming", true }, 
	        { "seems", true }, 
	        { "seen", true }, 
	        { "self", true }, 
	        { "selves", true }, 
	        { "sent", true }, 
	        { "seven", true }, 
	        { "several", true }, 
	        { "shall", true }, 
	        { "she", true }, 
	        { "shed", true }, 
	        { "she'll", true }, 
	        { "shes", true }, 
	        { "should", true }, 
	        { "shouldn't", true }, 
	        { "show", true }, 
	        { "showed", true }, 
	        { "shown", true }, 
	        { "showns", true }, 
	        { "shows", true }, 
	        { "significant", true }, 
	        { "significantly", true }, 
	        { "similar", true }, 
	        { "similarly", true }, 
	        { "since", true }, 
	        { "six", true }, 
	        { "slightly", true }, 
	        { "so", true }, 
	        { "some", true }, 
	        { "somebody", true }, 
	        { "somehow", true }, 
	        { "someone", true }, 
	        { "somethan", true }, 
	        { "something", true }, 
	        { "sometime", true }, 
	        { "sometimes", true }, 
	        { "somewhat", true }, 
	        { "somewhere", true }, 
	        { "soon", true }, 
	        { "sorry", true }, 
	        { "specifically", true }, 
	        { "specified", true }, 
	        { "specify", true }, 
	        { "specifying", true }, 
	        { "still", true }, 
	        { "stop", true }, 
	        { "strongly", true }, 
	        { "sub", true }, 
	        { "substantially", true }, 
	        { "successfully", true }, 
	        { "such", true }, 
	        { "sufficiently", true }, 
	        { "suggest", true }, 
	        { "sup", true }, 
	        { "sure	t", true }, 
	        { "take", true }, 
	        { "taken", true }, 
	        { "taking", true }, 
	        { "tell", true }, 
	        { "tends", true }, 
	        { "th", true }, 
	        { "than", true }, 
	        { "thank", true }, 
	        { "thanks", true }, 
	        { "thanx", true }, 
	        { "that", true }, 
	        { "that'll", true }, 
	        { "thats", true }, 
	        { "that've", true }, 
	        { "the", true }, 
	        { "their", true }, 
	        { "theirs", true }, 
	        { "them", true }, 
	        { "themselves", true }, 
	        { "then", true }, 
	        { "thence", true }, 
	        { "there", true }, 
	        { "thereafter", true }, 
	        { "thereby", true }, 
	        { "thered", true }, 
	        { "therefore", true }, 
	        { "therein", true }, 
	        { "there'll", true }, 
	        { "thereof", true }, 
	        { "therere", true }, 
	        { "theres", true }, 
	        { "thereto", true }, 
	        { "thereupon", true }, 
	        { "there've", true }, 
	        { "these", true }, 
	        { "they", true }, 
	        { "theyd", true }, 
	        { "they'll", true }, 
	        { "theyre", true }, 
	        { "they've", true }, 
	        { "think", true }, 
	        { "this", true }, 
	        { "those", true }, 
	        { "thou", true }, 
	        { "though", true }, 
	        { "thoughh", true }, 
	        { "thousand", true }, 
	        { "throug", true }, 
	        { "through", true }, 
	        { "throughout", true }, 
	        { "thru", true }, 
	        { "thus", true }, 
	        { "til", true }, 
	        { "tip", true }, 
	        { "to", true }, 
	        { "together", true }, 
	        { "too", true }, 
	        { "took", true }, 
	        { "toward", true }, 
	        { "towards", true }, 
	        { "tried", true }, 
	        { "tries", true }, 
	        { "truly", true }, 
	        { "try", true }, 
	        { "trying", true }, 
	        { "ts", true }, 
	        { "twice", true }, 
	        { "two", true }, 
	        { "u", true }, 
	        { "un", true }, 
	        { "under", true }, 
	        { "unfortunately", true }, 
	        { "unless", true }, 
	        { "unlike", true }, 
	        { "unlikely", true }, 
	        { "until", true }, 
	        { "unto", true }, 
	        { "up", true }, 
	        { "upon", true }, 
	        { "ups", true }, 
	        { "us", true }, 
	        { "use", true }, 
	        { "used", true }, 
	        { "useful", true }, 
	        { "usefully", true }, 
	        { "usefulness", true }, 
	        { "uses", true }, 
	        { "using", true }, 
	        { "usually", true }, 
	        { "v", true }, 
	        { "value", true }, 
	        { "various", true }, 
	        { "'ve", true }, 
	        { "very", true }, 
	        { "via", true }, 
	        { "viz", true }, 
	        { "vol", true }, 
	        { "vols", true }, 
	        { "vs", true }, 
	        { "w", true }, 
	        { "want", true }, 
	        { "wants", true }, 
	        { "was", true }, 
	        { "wasnt", true }, 
	        { "way", true }, 
	        { "we", true }, 
	        { "wed", true }, 
	        { "welcome", true }, 
	        { "we'll", true }, 
	        { "went", true }, 
	        { "were", true }, 
	        { "werent", true }, 
	        { "we've", true }, 
	        { "what", true }, 
	        { "whatever", true }, 
	        { "what'll", true }, 
	        { "whats", true }, 
	        { "when", true }, 
	        { "whence", true }, 
	        { "whenever", true }, 
	        { "where", true }, 
	        { "whereafter", true }, 
	        { "whereas", true }, 
	        { "whereby", true }, 
	        { "wherein", true }, 
	        { "wheres", true }, 
	        { "whereupon", true }, 
	        { "wherever", true }, 
	        { "whether", true }, 
	        { "which", true }, 
	        { "while", true }, 
	        { "whim", true }, 
	        { "whither", true }, 
	        { "who", true }, 
	        { "whod", true }, 
	        { "whoever", true }, 
	        { "whole", true }, 
	        { "who'll", true }, 
	        { "whom", true }, 
	        { "whomever", true }, 
	        { "whos", true }, 
	        { "whose", true }, 
	        { "why", true }, 
	        { "widely", true }, 
	        { "willing", true }, 
	        { "wish", true }, 
	        { "with", true }, 
	        { "within", true }, 
	        { "without", true }, 
	        { "wont", true }, 
	        { "words", true }, 
	        { "world", true }, 
	        { "would", true }, 
	        { "wouldnt", true }, 
	        { "www", true }, 
	        { "x", true }, 
	        { "y", true }, 
	        { "yes", true }, 
	        { "yet", true }, 
	        { "you", true }, 
	        { "youd", true }, 
	        { "you'll", true }, 
	        { "your", true }, 
	        { "youre", true }, 
	        { "yours", true }, 
	        { "yourself", true }, 
	        { "yourselves", true }, 
	        { "you've", true }, 
	        { "z", true }, 
	        { "zero", true }
        };

    }
}
/*
 * stop word list from http://www.ranks.nl/stopwords
 * 
a
able
about
above
abst
accordance
according
accordingly
across
act
actually
added
adj
affected
affecting
affects
after
afterwards
again
against
ah
all
almost
alone
along
already
also
although
always
am
among
amongst
an
and
announce
another
any
anybody
anyhow
anymore
anyone
anything
anyway
anyways
anywhere
apparently
approximately
are
aren
arent
arise
around
as
aside
ask
asking
at
auth
available
away
awfully
b
back
be
became
because
become
becomes
becoming
been
before
beforehand
begin
beginning
beginnings
begins
behind
being
believe
below
beside
besides
between
beyond
biol
both
brief
briefly
but
by
c
ca
came
can
cannot
can't
cause
causes
certain
certainly
co
com
come
comes
contain
containing
contains
could
couldnt
d
date
did
didn't
different
do
does
doesn't
doing
done
don't
down
downwards
due
during
e
each
ed
edu
effect
eg
eight
eighty
either
else
elsewhere
end
ending
enough
especially
et
et-al
etc
even
ever
every
everybody
everyone
everything
everywhere
ex
except
f
far
few
ff
fifth
first
five
fix
followed
following
follows
for
former
formerly
forth
found
four
from
further
furthermore
g
gave
get
gets
getting
give
given
gives
giving
go
goes
gone
got
gotten
h
had
happens
hardly
has
hasn't
have
haven't
having
he
hed
hence
her
here
hereafter
hereby
herein
heres
hereupon
hers
herself
hes
hi
hid
him
himself
his
hither
home
how
howbeit
however
hundred
i
id
ie
if
i'll
im
immediate
immediately
importance
important
in
inc
indeed
index
information
instead
into
invention
inward
is
isn't
it
itd
it'll
its
itself
i've
j
just
k
keep	keeps
kept
kg
km
know
known
knows
l
largely
last
lately
later
latter
latterly
least
less
lest
let
lets
like
liked
likely
line
little
'll
look
looking
looks
ltd
m
made
mainly
make
makes
many
may
maybe
me
mean
means
meantime
meanwhile
merely
mg
might
million
miss
ml
more
moreover
most
mostly
mr
mrs
much
mug
must
my
myself
n
na
name
namely
nay
nd
near
nearly
necessarily
necessary
need
needs
neither
never
nevertheless
new
next
nine
ninety
no
nobody
non
none
nonetheless
noone
nor
normally
nos
not
noted
nothing
now
nowhere
o
obtain
obtained
obviously
of
off
often
oh
ok
okay
old
omitted
on
once
one
ones
only
onto
or
ord
other
others
otherwise
ought
our
ours
ourselves
out
outside
over
overall
owing
own
p
page
pages
part
particular
particularly
past
per
perhaps
placed
please
plus
poorly
possible
possibly
potentially
pp
predominantly
present
previously
primarily
probably
promptly
proud
provides
put
q
que
quickly
quite
qv
r
ran
rather
rd
re
readily
really
recent
recently
ref
refs
regarding
regardless
regards
related
relatively
research
respectively
resulted
resulting
results
right
run
s
said
same
saw
say
saying
says
sec
section
see
seeing
seem
seemed
seeming
seems
seen
self
selves
sent
seven
several
shall
she
shed
she'll
shes
should
shouldn't
show
showed
shown
showns
shows
significant
significantly
similar
similarly
since
six
slightly
so
some
somebody
somehow
someone
somethan
something
sometime
sometimes
somewhat
somewhere
soon
sorry
specifically
specified
specify
specifying
still
stop
strongly
sub
substantially
successfully
such
sufficiently
suggest
sup
sure	t
take
taken
taking
tell
tends
th
than
thank
thanks
thanx
that
that'll
thats
that've
the
their
theirs
them
themselves
then
thence
there
thereafter
thereby
thered
therefore
therein
there'll
thereof
therere
theres
thereto
thereupon
there've
these
they
theyd
they'll
theyre
they've
think
this
those
thou
though
thoughh
thousand
throug
through
throughout
thru
thus
til
tip
to
together
too
took
toward
towards
tried
tries
truly
try
trying
ts
twice
two
u
un
under
unfortunately
unless
unlike
unlikely
until
unto
up
upon
ups
us
use
used
useful
usefully
usefulness
uses
using
usually
v
value
various
've
very
via
viz
vol
vols
vs
w
want
wants
was
wasnt
way
we
wed
welcome
we'll
went
were
werent
we've
what
whatever
what'll
whats
when
whence
whenever
where
whereafter
whereas
whereby
wherein
wheres
whereupon
wherever
whether
which
while
whim
whither
who
whod
whoever
whole
who'll
whom
whomever
whos
whose
why
widely
willing
wish
with
within
without
wont
words
world
would
wouldnt
www
x
y
yes
yet
you
youd
you'll
your
youre
yours
yourself
yourselves
you've
z
zero
*/
