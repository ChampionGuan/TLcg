Bezier = function()
    local t = {}

    t.startPos = nil
    t.endPos = nil
    t.middlePos = nil
    t.distance = nil
    t.resultPos = CSharp.Vector3.zero

    function t:Clear()
        self.startPos = nil
        self.endPos = nil
        self.middlePos = nil
        self.distance = nil
        self.resultPos = CSharp.Vector3.zero
    end

    function t:SetBezierPoints(startPos, endPos, heightFactor)
        self:Clear()
        self.distance = CSharp.Vector3.Distance(startPos, endPos)
        self.startPos = startPos
        self.endPos = endPos
        self.middlePos = 0.5 * (startPos + endPos) + CSharp.Vector3(0, self.distance, 0) * heightFactor
    end

    function t:SetLinePoints(startPos, endPos)
        self:Clear()
        self.startPos = startPos
        self.endPos = endPos
        self.middlePos = 0.5 * (startPos + endPos)
    end

    function t:GetPointAtTime(value)
        if value > 1 then
            value = 1
        end
        if value < 0 then
            value = 0
        end

        self.resultPos.x =
            value * value * (self.endPos.x - 2 * self.middlePos.x + self.startPos.x) + self.startPos.x +
            2 * value * (self.middlePos.x - self.startPos.x)
        self.resultPos.y =
            value * value * (self.endPos.y - 2 * self.middlePos.y + self.startPos.y) + self.startPos.y +
            2 * value * (self.middlePos.y - self.startPos.y)
        self.resultPos.z =
            value * value * (self.endPos.z - 2 * self.middlePos.z + self.startPos.z) + self.startPos.z +
            2 * value * (self.middlePos.z - self.startPos.z)

        return self.resultPos
    end

    return t
end
