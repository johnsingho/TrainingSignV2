
use TrainingSign;

/*==============================================================*/
/* Table: sys_user                                              */
/*==============================================================*/
create table sys_user (
   id                   int identity,
   ADAccount            varchar(50)          not null,
   FullName             varchar(50)          not null,
   Email                varchar(100)         not null,
   LastLogon            datetime             null,
   IsValid              bit                  not null default 1,
   IsAdmin              bit                  not null default 0,
   constraint PK_sys_user primary key (id)
)
go

/*==============================================================*/
/* Table: tbl_course                                            */
/*==============================================================*/
create table tbl_course (
   id                   UNIQUEIDENTIFIER     not null default newsequentialid(),
   course_no            varchar(40)          unique,
   course_context       nvarchar(200)        not null,
   course_time          float                null,
   constraint PK_tbl_course primary key (id)
)
go

/*==============================================================*/
/* Table: tbl_lector                                            */
/*==============================================================*/
create table tbl_lector (
   id                   UNIQUEIDENTIFIER     not null default newsequentialid(),
   lector_workid        varchar(10)          unique,
   lector_en_name       varchar(40)          not null,
   lector_cn_name       nvarchar(40)         not null,
   memo                 nvarchar(50)         null,
   constraint PK_tbl_lector primary key (id)
)
go

/*==============================================================*/
/* Table: tbl_lector_course_link                                */
/*==============================================================*/
create table tbl_lector_course_link (
   id                   int identity ,
   ref_lector_id        UNIQUEIDENTIFIER     not null,
   ref_course_id        UNIQUEIDENTIFIER     not null,
   constraint UNQ_tbl_lec_cou_link UNIQUE (ref_lector_id,ref_course_id),
   constraint PK_tbl_lector_course_link primary key (id)
)
go

/*==============================================================*/
/* Table: tbl_training_lector_link                              */
/*==============================================================*/
create table tbl_training_lector_link (
   id                   int identity ,   
   ref_training_id      UNIQUEIDENTIFIER     not null,
   ref_lector_id        UNIQUEIDENTIFIER     not null,
   constraint UNQ_tbl_tra_lec_link UNIQUE (ref_lector_id,ref_training_id),
   constraint PK_tbl_tra_lec_link primary key (id)
)
go

/*==============================================================*/
/* Table: tbl_trainee                                           */
/*==============================================================*/
create table tbl_trainee (
   id                   int identity,
   ref_training_id      UNIQUEIDENTIFIER     not null,
   workid				varchar(10)                   ,
   name					nvarchar(40)         not null,
   department			nvarchar(80)                  ,
   organizer            nvarchar(40)         null,
   signinTime			DateTime DEFAULT GETDATE(),
   memo                 nvarchar(50)         null,
   constraint UNQ_tbl_trainee UNIQUE (ref_training_id,workid),
   constraint PK_tbl_trainee primary key (id)
)
go

/*==============================================================*/
/* Table: tbl_training                                          */
/*==============================================================*/
create table tbl_training (
   id                   UNIQUEIDENTIFIER     not null default newsequentialid(),
   ref_course_id        UNIQUEIDENTIFIER     not null,
   trainer_organizer    nvarchar(40)         null,
   organizer            nvarchar(40)         null,
   venue                nvarchar(40)         null,
   plan_reach           int                  null default 0,
   actual_reach         int                  null default 0,
   total_training_time  float                null default 0.0,
   pass                 int                  null default 0,
   plan_start_time      datetime             null default getdate(),
   plan_end_time        datetime             null,
   actual_end_time      datetime             null,
   end_lector_workid    varchar(10)          null,
   constraint PK_tbl_training primary key (id)
)
go

-- 删除人员签到记录
create table tbl_delete_trainee_log (
   id                   int identity,
   ref_training_id      UNIQUEIDENTIFIER     not null,
   workid				varchar(10)                   ,
   name					nvarchar(40)         not null,
   signinTime			DateTime,
   deleteTime			DateTime DEFAULT GETDATE(),
   constraint PK_delete_trainee_log primary key (id)
)
go


alter table tbl_lector_course_link
   add constraint FK_TBL_LECT_REFERENCE_TBL_COUR foreign key (ref_course_id)
      references tbl_course (id)
go

alter table tbl_lector_course_link
   add constraint FK_TBL_LECT_REFERENCE_TBL_LECT foreign key (ref_lector_id)
      references tbl_lector (id)
go

alter table tbl_training_lector_link
   add constraint FK_TBL_TRA_LECT_REFERENCE_TBL_TRAIN foreign key (ref_training_id)
      references tbl_training (id)
go


alter table tbl_training_lector_link
   add constraint FK_TBL_TRA_LECT_REFERENCE_TBL_LECT foreign key (ref_lector_id)
      references tbl_lector (id)
go

alter table tbl_trainee
   add constraint FK_TBL_TRAI_REFERENCE_TBL_TRAI foreign key (ref_training_id)
      references tbl_training (id)
go


-- 合并此培训的所有讲师名为一条字符串
CREATE FUNCTION dbo.Get_training_lectors_en(@training_id varchar(200))
RETURNS VARCHAR(400)
AS   
BEGIN  
	DECLARE @lector_en varchar(60);
	DECLARE @ret_lectors varchar(400)='';
	DECLARE cur_lector CURSOR FOR 
		select lec.lector_en_name
		from tbl_training t, tbl_training_lector_link tl, tbl_lector lec
		where t.id=tl.ref_training_id
		and tl.ref_lector_id=lec.id
		and t.id=@training_id;

	OPEN cur_lector
	FETCH NEXT FROM cur_lector INTO @lector_en

	WHILE @@FETCH_STATUS = 0  
	BEGIN 
		if len(@ret_lectors)>0 
		BEGIN
			SET @ret_lectors = @ret_lectors+ '/';	
		END
		SET @ret_lectors = @ret_lectors+ @lector_en;

		FETCH NEXT FROM cur_lector INTO @lector_en 
	END   
	CLOSE cur_lector;  
	DEALLOCATE cur_lector;  

	RETURN @ret_lectors;
END;
GO

CREATE FUNCTION dbo.Get_Training_Time_str(@dtStart datetime, @dtEnd datetime)
RETURNS VARCHAR(64)
AS
BEGIN
	DECLARE @ret VARCHAR(64)='';
	SET @ret =CONVERT(varchar(30), @dtStart, 120);
	SET @ret = @ret +' ~ ';
	SET @ret = @ret + CONVERT(varchar(30), @dtEnd, 120);
	RETURN @ret
END;
GO

------------------------------
-- 修正迁移到正式库过程中出现的问题

ALTER TABLE tbl_course
ADD CONSTRAINT UC_tbl_course_no UNIQUE (course_no);
GO

ALTER TABLE tbl_lector
ADD CONSTRAINT UC_tbl_lector_workid UNIQUE (lector_workid);
GO


ALTER TABLE tbl_course
alter column course_no varchar(40);

ALTER TABLE tbl_course
alter column course_context nvarchar(200) not null;



