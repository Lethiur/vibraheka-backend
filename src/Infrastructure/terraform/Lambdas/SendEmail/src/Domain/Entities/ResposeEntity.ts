
export default class ResponseEntity {
    
    constructor(public readonly statusCode: number, public readonly error: string | null, public readonly triggerSource : string) {
    }
    
    public static Success(triggerSource: string): ResponseEntity {
        return new ResponseEntity(200, null, triggerSource);
    }
    
    public static Error(error: string, triggerSource: string, statusCode: number): ResponseEntity {
        return new ResponseEntity(statusCode, error, triggerSource);
    }
}