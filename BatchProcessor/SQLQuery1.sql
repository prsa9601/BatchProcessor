SELECT 
    p.UserId, 
    COUNT(p.[Index]) AS CountIndex ,
	COUNT(*) as Total
FROM PostLikes AS p
GROUP BY p.UserId
ORDER BY CountIndex;

truncate table PostLikes