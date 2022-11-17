import React, {useEffect, useState} from 'react';
import {Button, Container, MantineProvider, Pagination, FileButton, Center, Group, TextInput} from "@mantine/core";
import './App.css';
import {GetGroups, GetGroupPagesCount, UploadLogFile, GroupEntry} from "./Api";
import GroupsTable from "./GroupsTable";
import {useDebouncedState, useDebouncedValue} from "@mantine/hooks";

function App() {
    const [groups, setGroups] = useState<GroupEntry[]>([]);
    const [activePage, setPage] = useState(0);
    const [searchText, setSearchText] = useDebouncedState('', 200);
    const [pageCount, setPageCount] = useState(20);

    useEffect(() => {
        GetGroups(activePage, searchText, setGroups);
        GetGroupPagesCount(setPageCount);
    }, [activePage, searchText]);

    return (
        <MantineProvider withGlobalStyles withNormalizeCSS>
            <Container fluid>
                <Center>
                    <Group>
                        <Pagination page={activePage} onChange={setPage} total={pageCount} siblings={3} initialPage={10}/>
                        <TextInput
                            defaultValue={searchText}
                            onChange={e => setSearchText(e.currentTarget.value)}
                            variant={"filled"} radius={"xl"} size={"md"} placeholder={"text"} />
                    </Group>
                </Center>
                
                { groups && <GroupsTable groups={groups} setGroups={setGroups}/> }
                
                <UploadLogs/>
            </Container>
        </MantineProvider>
    );
}

function UploadLogs() {
    return (
        <FileButton onChange={UploadLogFile}>
            {(props) => <Button {...props}>Upload logs</Button>}
        </FileButton>
    )
}

export default App;
