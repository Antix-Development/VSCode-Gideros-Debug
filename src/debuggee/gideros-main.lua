require 'strict'

-------------------------------------------------------------------------------
-- UI 출력 코드입니다.
local dfont = TTFont.new('c:\\Windows\\Fonts\\malgunbd.ttf', 12)
local logs = {}
local function log(str)
	if #logs > 27 then
		logs[1]:removeFromParent()
		table.remove(logs, 1)
	end

	local text = TextField.new(dfont, str)
	stage:addChild(text)
	table.insert(logs, text)
	
	for i, text in ipairs(logs) do
		text:setPosition(16, 12 + 16 * i)
	end
end

require 'json'
local debuggee = require 'vscode-debuggee'
local startResult, breakerType = debuggee.start(json, { redirectPrint = true })
print('debuggee start ->', startResult, breakerType)
log("됐어, 연결됐어")

local timer = Timer.new(1000)
timer:addEventListener(Event.TIMER, function()
	debuggee.poll()
	print("한글!!")
	print("왜날뷁!!")
	print("")
	print(1,2,3,4,5)
	log(os.timer())
end)
timer:start()


