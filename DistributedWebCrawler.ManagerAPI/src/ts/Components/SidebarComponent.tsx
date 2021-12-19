import React from "react";
import { Link, useLocation } from "react-router-dom";
import { ProSidebar, Menu, MenuItem, SubMenu } from "react-pro-sidebar";
import { Props as ProSidebarProps } from "react-pro-sidebar/dist/ProSidebar/ProSidebar";
import { ComponentDescription } from "./AppComponent";

export interface SidebarProps extends ProSidebarProps {
  componentDescriptions: ComponentDescription[];
}

const Sidebar: React.FC<SidebarProps> = ({
  componentDescriptions: componentDescriptions,
  collapsed,
  toggled,
  onToggle,
}) => {
  const location = useLocation();
  return (
    <ProSidebar
      rtl={false}
      collapsed={collapsed}
      toggled={toggled}
      breakPoint="md"
      onToggle={onToggle}
    >
      {/* <SidebarHeader>Web Crawler</SidebarHeader> */}
      <Menu iconShape="square">
        <MenuItem active={location.pathname == "/"}>
          Dashboard
          <Link to="/" />
        </MenuItem>
        <SubMenu
          title="Components"
          defaultOpen={location.pathname.startsWith("/component/")}
        >
          {componentDescriptions.map((component, i) => (
            <MenuItem
              key={i.toString()}
              active={location.pathname == `/component/${component.name}`}
            >
              {component.friendlyName}
              <Link to={`/component/${component.name}`} />
            </MenuItem>
          ))}
        </SubMenu>
      </Menu>
    </ProSidebar>
  );
};

export default Sidebar;
