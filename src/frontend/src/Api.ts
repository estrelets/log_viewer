import axios, {AxiosRequestConfig} from "axios";
import {Simulate} from "react-dom/test-utils";
import error = Simulate.error;

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

const host = '';

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

export function GetGroups(page: number, text:string|undefined, onLoad: (g: GroupEntry[]) => void) {
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
                const data2 = (data as GroupEntry[])
                console.log('success');
                onLoad(data2);
            },
            error => {
                console.log(error);
            })
}

export function GetGroupPagesCount(onLoad: (count: number) => void) {
    fetch(`${host}/Locator/groups/count`)
        .then(res => {
            if (!res.ok) throw Error(res.statusText);
            return res.json();
        })
        .then(
            data => {
                onLoad(Math.floor(data / pageSize));
            },
            error => {
                console.log(error);
            })
}