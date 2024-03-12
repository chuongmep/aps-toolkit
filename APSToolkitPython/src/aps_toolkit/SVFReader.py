class SVFReader:
    def __init__(self, filename):
        self.filename = filename
        self.file = open(filename, 'r')
        self.lines = self.file.readlines()
        self.file.close()
        self.line = 0
        self.current = self.lines[self.line]

    def next(self):
        self.line += 1
        if self.line < len(self.lines):
            self.current = self.lines[self.line]
            return self.current
        else:
            return None

    def get(self):
        return self.current

    def getLine(self):
        return self.line

    def getFilename(self):
        return self.filename

    def getLines(self):
        return self.lines

    def close(self):
        self.file.close()
        self.file = None
        self.lines = None
        self.current = None
        self.line = None
        self.filename = None