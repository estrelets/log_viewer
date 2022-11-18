import axios, {AxiosRequestConfig} from "axios";
import {Simulate} from "react-dom/test-utils";
import error = Simulate.error;

export interface GetGroupResultDto {
    total: number,
    groups: GroupEntry[]
}

export interface GroupEntry {
    line: number,
    start: string,
    end: string,
    message: string,
    duration: number,
    logs: LogEntry[],
    type: number,
    expand: boolean,
}

export interface LogEntry {
    line: number,
    time: string,
    type: number,
    requestId: string,
    messagePreview: string,
    duration: number
}

const host = 'http://localhost:5999';

export function UploadLogFile(file: File) {
    const url = `${host}/Reader/upload`;
    const formData = new FormData();
    formData.append('file', file);
    const config: AxiosRequestConfig = {
        headers: {
            'Content-Type': 'multipart/form-data',
        },
    };
    axios
        .post(url, formData, config)
        .then((response) => {
            console.log(response.data);
        });
}

const pageSize = 100;

export function GetGroups(
    page: number,
    text:string|undefined,
    setGroups: (g: GroupEntry[]) => void,
    setCount: (c: number) => void)
{
    const skip = (page - 1) * pageSize;

    let url = `${host}/Locator/groups?skip=${skip}&take=${pageSize}`;

    if(text){
        url += `&text=${text}`;
    }

    fetch(url)
        .then(res => {
            if (!res.ok) throw Error(res.statusText);
            return res.json();
        })
        .then(
            data => {
                const result = data as GetGroupResultDto;
                console.log(result);
                setGroups(result.groups);
                const pages = Math.ceil(result.total / pageSize);
                setCount(pages)
            },
            error => {
                console.log(error);
            })
}
