using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using dotnetfsample.Converters;


namespace dotnetfsample.Dtos.TestDto
{
    public class ErpDto
    {
        public string IV_BUKRS { get; set; }

        // 옵션 항목 (값이 없는경우 항목제거하여 전송)
        //public string? EXT_FIELD { get; set; }

        // 싱글 레코드 (싱글인 경우 [] 제거하여 전송)
        public IT_HEAD IT_HEAD { get; set; } = new IT_HEAD();

        // 옵션 테이블 (값이 없는경우 항목제거하여 전송)
        //public IT_ITEM? IT_ITEM { get; set; }
    }

    public class IT_HEAD
    {
        public string ZDEDN { get; set; }

        // 옵션 항목 (값이 없는경우 항목제거하여 전송)
        //public string? ZINTEID { get; set; }

        // 날짜 타입 (DATS 타입을 JSON 변환 시 문자열로 전송)
        public string APPDAT { get; set; }
    }

    public class IT_ITEM
    {
        public string ZDEDN { get; set; }
        public string ZPOSNR { get; set; }
    }



    public class LegacyDto
    {
        // 항목명 맵핑 (ERP : IV_BUKRS  ->  Legacy : BUKRS)
        [JsonProperty("IV_BUKRS")]
        public string BUKRS { get; set; }

        // 필요 항목 (항목이 없더라도 항목 유지하고 값은 NULL)
        public string EXT_FIELD { get; set; }


        private ERP_BILL_I_HEAD[] _itHead = Array.Empty<ERP_BILL_I_HEAD>();

        // 멀티 레코드 (싱글인 경우 Array 로 변환)
        [JsonConverter(typeof(SingleArrayJsonConverter<ERP_BILL_I_HEAD>))]
        public ERP_BILL_I_HEAD[] IT_HEAD
        {
            get => _itHead;
            set => _itHead = value ?? _itHead; // null 방어
        }

        private ERP_BILL_I_LIST _itItem = new ERP_BILL_I_LIST();

        public ERP_BILL_I_LIST IT_ITEM
        {
            get => _itItem;
            set => _itItem = value ?? _itItem; //null 방어
        }
    }

    public class ERP_BILL_I_HEAD
    {
        public string ZDEDN { get; set; }

        // 필요 항목 (항목이 없더라도 항목 유지하고 값은 NULL)
        public string ZINTEID { get; set; }

        // 날짜 타입 (날짜포멧 문자열을 DateTime 타입으로 변환)
        [JsonConverter(typeof(SapDatsConverter))]
        public DateTime? APPDAT { get; set; }
    }

    public class ERP_BILL_I_LIST
    {
        public string ZDEDN { get; set; }
        public string ZPOSNR { get; set; }
    }
}