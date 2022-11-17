import {ActionIcon, Badge, Center, Group, MantineColor, Table, Text} from "@mantine/core";
import React, {useState} from "react";
import {GroupEntry, LogEntry} from "./Api";
import {IconChevronDown, IconChevronUp} from "@tabler/icons";
import GroupModal from "./GroupModal";

const mapColor = (type: number) => type == 0 ? '#ffe3e3' : 'transparent';

function GroupsTable(args:{groups:GroupEntry[], setGroups:(groups:GroupEntry[]) => void}) {
    const expand = (group: GroupEntry) => {
        const index = findPosition(group);
        args.groups[index] = {...group, expand: !group.expand};
        args.setGroups([...args.groups]);
    }

    const findPosition = (group: GroupEntry) => {
        return args.groups.indexOf(group);
    }

    const mapLog = (l: LogEntry) => (
            <tr key={l.line} style={{backgroundColor: mapColor(l.type)}}>
                <td colSpan={2} style={{paddingLeft: '30px'}}> &gt;&gt; {l.time}</td>
                <td colSpan={2}>
                    {l.messagePreview.substring(0, 150)}
                </td>
            </tr>
        );
    
    const toggleChilds = (group: GroupEntry) => (<>
        {(group.logs.length > 1 && !group.expand) &&
            <ActionIcon onClick={_ => expand(group)}>
                <IconChevronDown></IconChevronDown>
            </ActionIcon>
        }
        {group.expand &&
            <ActionIcon onClick={_ => expand(group)}>
                <IconChevronUp></IconChevronUp>
            </ActionIcon>
        }
        </>);

    const mapTitleColor  = (duration: number): MantineColor => {
        if(duration < 200) return 'green';
        if(duration < 600) return 'yellow'
        return 'red';
    };
    
    const mapGroup = (group: GroupEntry) => (
            <>
                <tr key={group.line} style={{backgroundColor: mapColor(group.type)}}>
                    <td>
                        <Group>
                            <Text>
                            {group.start.substring(5, 19).replace('T', ' ')}
                            </Text>
                            <Badge title={group.end} color={mapTitleColor(group.duration)}>
                                {group.duration}
                            </Badge>
                        </Group>
                    </td>
                    <td>
                        <Group>
                            {toggleChilds(group)}
                            <GroupModal group={group}/>
                        </Group>
                    </td>
                    <td>{group.message}</td>
                </tr>
                {group.expand &&
                    <>
                        {group.logs.map(l => mapLog(l))}
                        <tr key={group.line + 'slit2'} style={{backgroundColor: '#fcfae1'}}>
                            <td colSpan={4}>
                                <Center>
                                    <ActionIcon onClick={_ => expand(group)}>
                                        <IconChevronUp></IconChevronUp>
                                    </ActionIcon>
                                </Center>
                            </td>
                        </tr>
                    </>
                }
            </>);

    const rows = args.groups.map(x => mapGroup(x));
    
    return (
        <Table striped>
            <colgroup>
                <col span={1} style={{width: '190px'}}/>
                <col span={1} style={{width: '110px'}}/>
                <col span={1}/>
            </colgroup>
            
            <thead>
                <tr>
                    <td>Start</td>
                    <td></td>
                    <td>Message</td>
                </tr>
            </thead>
            
            <tbody>
                {rows}
            </tbody>
        </Table>
    )
} 

export default GroupsTable;