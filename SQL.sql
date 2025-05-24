-- Thêm column phần trăm tiền cọc vào tour
alter table Tours add PercentDeposit int
-- Thêm column phần trăm tiền cọc vào room
alter table Rooms add PercentDeposit int
-- Thêm column duyệt dịch vụ
alter table Bills add IsActive bit
--Update 
update Tours
set PercentDeposit = 20
where Id = '8959BADB-9CAD-454D-B38C-035A620CBBAC'
--Update 
update Rooms
set PercentDeposit = 20
where Id = '76b2cb1c-ef54-4424-aa1e-acb2f0c5d921'