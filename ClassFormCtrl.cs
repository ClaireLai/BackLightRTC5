using System.Windows.Forms;
using System.Drawing;

namespace BackLight
{
    public enum EVENT_TYPE { UPDATE_BUTTON, UPDATE_TEXTBOX, APPEND_TEXTBOX, INSERT_TEXTBOX, UPDATE_LABLE, UPDATE_PICBOX, UPDATE_CHKBOX, BIND_DATAGRIDVIEW, MODIFY_DATAGRIDVIEW_PROCESSPOINT, BIND_DATAGRIDVIEW_MEMBER }

    public class ClassFormCtrl
    {
        private delegate void EventHandlerPtr(EVENT_TYPE type, object obj, object data);
        public void eventHandler(EVENT_TYPE type, object obj, object data)
        {
            switch (type)
            {
                case EVENT_TYPE.UPDATE_BUTTON:
                    updateButton((Button)obj, (CtrlItemSetting)data);
                    break;

                case EVENT_TYPE.UPDATE_LABLE:
                    updateLable((Label)obj, (CtrlItemSetting)data);
                    break;

                case EVENT_TYPE.UPDATE_TEXTBOX:
                    updateTextBox((TextBox)obj, (CtrlItemSetting)data);
                    break;

                case EVENT_TYPE.APPEND_TEXTBOX:
                    appendTextBox((TextBox)obj, (CtrlItemSetting)data);
                    break;

                case EVENT_TYPE.UPDATE_PICBOX:
                    updatePictureBox((PictureBox)obj, (CtrlItemSetting)data);
                    break;

                case EVENT_TYPE.UPDATE_CHKBOX:
                    updateCheckbox((CheckBox)obj, (ChkBoxSetting)data);
                    break;

                case EVENT_TYPE.BIND_DATAGRIDVIEW:
                    bindDataGridView((DataGridView)obj, data);
                    break;
            }
        }
        private void updateButton(Button btn, CtrlItemSetting button_setting)
        {
            if (btn.InvokeRequired)
            {
                EventHandlerPtr et_handler = eventHandler;
                btn.BeginInvoke(et_handler, EVENT_TYPE.UPDATE_BUTTON, btn, button_setting);
            }
            else
            {
                btn.Enabled = button_setting.enabled;
            }
        }
        private void updateLable(Label label, CtrlItemSetting label_setting)
        {
            if (label.InvokeRequired)
            {
                EventHandlerPtr et_handler = eventHandler;
                label.BeginInvoke(et_handler, EVENT_TYPE.UPDATE_LABLE, label, label_setting);
            }
            else
            {
                label.Text = label_setting.text;

                if (label_setting.background != null)
                    label.ForeColor = ((Color)label_setting.background);

            }
        }
        private void updateTextBox(TextBox textbox, CtrlItemSetting textbox_setting)
        {
            if (textbox.InvokeRequired)
            {
                EventHandlerPtr et_handler = eventHandler;
                textbox.BeginInvoke(et_handler, EVENT_TYPE.UPDATE_TEXTBOX, textbox, textbox_setting);
            }
            else
            {
                textbox.Enabled = textbox_setting.enabled;

                textbox.Text = textbox_setting.text;

                if (textbox_setting.background != null)
                    textbox.BackColor = ((Color)textbox_setting.background);
            }
        }
        private void appendTextBox(TextBox textbox, CtrlItemSetting textbox_setting)
        {
            if (textbox.InvokeRequired)
            {
                EventHandlerPtr et_handler = eventHandler;
                textbox.BeginInvoke(et_handler, EVENT_TYPE.APPEND_TEXTBOX, textbox, textbox_setting);
            }
            else
            {
                textbox.Enabled = textbox_setting.enabled;

                textbox.Text += textbox_setting.text;

                if (textbox_setting.background != null)
                    textbox.BackColor = ((Color)textbox_setting.background);
            }
        }
        private void insertTextBox(TextBox textbox, CtrlItemSetting textbox_setting)
        {
            if (textbox.InvokeRequired)
            {
                EventHandlerPtr et_handler = eventHandler;
                textbox.BeginInvoke(et_handler, EVENT_TYPE.APPEND_TEXTBOX, textbox, textbox_setting);
            }
            else
            {
                textbox.Enabled = textbox_setting.enabled;

                textbox.Text.Insert(0,textbox_setting.text);

                if (textbox_setting.background != null)
                    textbox.BackColor = ((Color)textbox_setting.background);
            }
        }
        private void updatePictureBox(PictureBox picbox, CtrlItemSetting picboxSett)
        {
            if (picbox.InvokeRequired)
            {
                EventHandlerPtr et_handler = eventHandler;
                picbox.BeginInvoke(et_handler, EVENT_TYPE.UPDATE_PICBOX, picbox, picboxSett);
            }
            else
            {
                picbox.Image = Image.FromFile(picboxSett.text);

                if (picboxSett.background != null)
                    picbox.BackColor = ((Color)picboxSett.background);

            }
        }
        private void updateCheckbox(CheckBox checkbox, ChkBoxSetting checkbox_setting)
        {
            if (checkbox.InvokeRequired)
            {
                EventHandlerPtr et_handler = eventHandler;
                checkbox.BeginInvoke(et_handler, EVENT_TYPE.UPDATE_CHKBOX, checkbox, checkbox_setting);
            }
            else
            {
                checkbox.Enabled = checkbox_setting.enabled;

                if (checkbox_setting.isChecked != null)
                    checkbox.Checked = (bool)checkbox_setting.isChecked;

                if (checkbox_setting.background != null)
                    checkbox.Text = checkbox_setting.text;

                if (checkbox_setting.background != null)
                    checkbox.BackColor = ((Color)checkbox_setting.background);
            }
        }
        private void bindDataGridView(DataGridView dataGridView, object list)
        {
            if (dataGridView.InvokeRequired)
            {
                EventHandlerPtr et_handler = eventHandler;
                dataGridView.BeginInvoke(et_handler, EVENT_TYPE.BIND_DATAGRIDVIEW, dataGridView, list);
            }
            else
            {
                dataGridView.DataSource = null;
                dataGridView.DataSource = list;
                dataGridView.Update();
                dataGridView.Refresh();
            }
        }
    }
    public class CtrlItemSetting
    {
        public bool enabled { get; set; }

        public object background { get; set; }

        public string text { get; set; }

        public CtrlItemSetting(bool ctrl_enabled)
        {
            enabled = ctrl_enabled;
            background = null;
            text = null;
        }

        public CtrlItemSetting(bool ctrl_enabled, object ctrl_color)
        {
            enabled = ctrl_enabled;
            background = ctrl_color;
            text = null;
        }

        public CtrlItemSetting(bool ctrl_enabled, object ctrl_color, string ctrl_text)
        {
            enabled = ctrl_enabled;
            background = ctrl_color;
            text = ctrl_text;
        }
    }
    public class ChkBoxSetting
    {
        public bool enabled { get; set; }

        public object background { get; set; }

        public string text { get; set; }

        public object isChecked { get; set; }

        public ChkBoxSetting(bool ctrl_enabled)
        {
            enabled = ctrl_enabled;
            background = null;
            text = null;
            isChecked = null;
        }

        public ChkBoxSetting(bool ctrl_enabled, bool ctrl_checked)
        {
            enabled = ctrl_enabled;
            isChecked = ctrl_checked;
            background = null;
            text = null;
        }

        public ChkBoxSetting(bool ctrl_enabled, bool ctrl_checked, object ctrl_color)
        {
            enabled = ctrl_enabled;
            isChecked = ctrl_checked;
            background = ctrl_color;
            text = null;
        }

        public ChkBoxSetting(bool ctrl_enabled, bool ctrl_checked, object ctrl_color, string ctrl_text)
        {
            enabled = ctrl_enabled;
            isChecked = ctrl_checked;
            background = ctrl_color;
            text = ctrl_text;
        }
    }
}
