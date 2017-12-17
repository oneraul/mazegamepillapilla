function string:split( inSplitPattern, outResults )
  if not outResults then
    outResults = { }
  end
  local theStart = 1
  local theSplitStart, theSplitEnd = string.find( self, inSplitPattern, theStart )
  while theSplitStart do
    table.insert( outResults, string.sub( self, theStart, theSplitStart-1 ) )
    theStart = theSplitEnd + 1
    theSplitStart, theSplitEnd = string.find( self, inSplitPattern, theStart )
  end
  table.insert( outResults, string.sub( self, theStart ) )
  return outResults
end

local dictionary = {}
    dictionary[-1] =  0
	dictionary[ 0] =  0
	dictionary[ 1] =  1
	dictionary[ 2] =  5
	dictionary[ 3] =  2
	dictionary[ 4] = 25
	dictionary[ 5] = 18
	dictionary[ 6] =  9
	dictionary[ 7] =  6
	dictionary[ 8] =  4
	dictionary[ 9] =  3
	dictionary[10] = 24
	dictionary[11] = 19
	dictionary[12] =  8
	dictionary[13] =  7
	dictionary[14] =  0		-- invalid
	dictionary[15] =  0		-- invalid
	dictionary[16] = 23
	dictionary[17] = 20
	dictionary[18] = 16
	dictionary[19] = 17
	dictionary[20] = 10
	dictionary[21] = 11
	dictionary[22] = 22
	dictionary[23] = 21
	dictionary[24] = 15
	dictionary[25] = 14
	dictionary[26] = 13
	dictionary[27] = 12
	dictionary[28] =  0		-- invalid
	dictionary[29] =  0		-- invalid


local fileIndex = 0
while true do
	local inputFile = io.open(fileIndex .. ".csv", "r")
	if inputFile == nil then break end

	local rawdata = {}
	for line in inputFile:lines() do
		local row = {}
		table.insert(rawdata, row)
		for k, v in ipairs(line:split(",")) do
			table.insert(row, dictionary[tonumber(v)])
		end
	end

	local outputFile = io.open("correct/" .. fileIndex .. ".corrected.csv", "w")
	for y, row in ipairs(rawdata) do
		for x, value in ipairs(row) do
			outputFile:write(value .. ",")
		end
		outputFile:write("\n")
	end
	outputFile:close()
	
	fileIndex = fileIndex+1
end

print("Processed " .. fileIndex .. " files.")